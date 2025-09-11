// scripts/cucumber-summary.js
// Usage: node scripts/cucumber-summary.js ./tests/.../reports/cucumber-report.json

const fs = require("fs");
const path = require("path");

const input = process.argv[2] || "reports/cucumber-report.json";
const outPath = process.env.GITHUB_STEP_SUMMARY;

function nsToMsMaybe(v) {
  // Cucumber JSON may report nanoseconds or milliseconds depending on version/formatter.
  // Heuristic: if it's big (e.g. > 10^7) treat as ns; else ms.
  const n = Number(v || 0);
  if (!isFinite(n) || n <= 0) return 0;
  return n > 10_000_000 ? n / 1e6 : n; // ns -> ms
}
function msFmt(ms) {
  if (!isFinite(ms)) return "0ms";
  if (ms >= 60_000) {
    const m = Math.floor(ms / 60_000);
    const s = Math.round((ms % 60_000) / 1000);
    return `${m}m ${s}s`;
  }
  if (ms >= 1000) return `${(ms / 1000).toFixed(2)}s`;
  return `${Math.round(ms)}ms`;
}
const tick = "✅";
const cross = "❌";
const warn = "⚠️";

function readJSON(p) {
  return JSON.parse(fs.readFileSync(p, "utf8"));
}

const data = readJSON(input);

// Aggregates
let totalFeatures = 0;
let totalScenarios = 0, passedScenarios = 0, failedScenarios = 0, skippedScenarios = 0;
let totalSteps = 0, passedSteps = 0, failedSteps = 0, skippedSteps = 0, pendingSteps = 0, undefinedSteps = 0;
let totalDurationMs = 0;

const perFeature = new Map(); // name -> { scenarios, passed, failed, skipped, durationMs }
const scenarios = [];         // { feature, name, status, durationMs }
const failures = [];          // { feature, scenario, steps: [{ step, error }] }

function itemsOfFeature(feature) {
  // Support both legacy and newer shapes
  return feature.elements || feature.children || [];
}
function stepsOf(el) {
  return el.steps || [];
}
function hooksOf(el, key) {
  // key: "before" | "after"
  return el[key] || [];
}
function isScenarioLike(el) {
  // Skip Background and other non-scenario entries
  const type = (el.type || el.keyword || "").toString().toLowerCase();
  if (type.includes("background")) return false;
  if (stepsOf(el).length) return true;
  // Some formatters leave steps empty when a hook hard-fails—still treat as scenario
  return !!el.name;
}

for (const feature of data) {
  const featureName = feature.name || "(unnamed feature)";
  totalFeatures++;

  const items = itemsOfFeature(feature);
  let featDuration = 0, featScenarios = 0, featPassed = 0, featFailed = 0, featSkipped = 0;

  for (const el of items) {
    if (!isScenarioLike(el)) continue;

    const scenarioName = el.name || "(unnamed scenario)";
    const stepList = stepsOf(el);
    const beforeHooks = hooksOf(el, "before");
    const afterHooks = hooksOf(el, "after");

    let scenDurationMs = 0;
    let scenStatus = "passed";
    const scenFailedDetails = [];

    // Steps
    for (const step of stepList) {
      const result = step.result || {};
      const status = (result.status || "unknown").toLowerCase();
      const durMs = nsToMsMaybe(result.duration);

      totalSteps++;
      totalDurationMs += durMs;
      scenDurationMs += durMs;

      if (status === "passed") passedSteps++;
      else if (status === "failed") {
        failedSteps++;
        scenStatus = "failed";
        scenFailedDetails.push({
          step: step.name || "(step)",
          error: (result.error_message || result.error || "").toString(),
        });
      } else if (status === "skipped") {
        skippedSteps++;
      } else if (status === "pending") {
        pendingSteps++;
        // Treat pending as failure for CI visibility
        scenStatus = scenStatus === "failed" ? "failed" : "failed";
        scenFailedDetails.push({
          step: step.name || "(pending step)",
          error: "Step is pending",
        });
      } else if (status === "undefined") {
        undefinedSteps++;
        // Treat undefined as failure for CI visibility
        scenStatus = scenStatus === "failed" ? "failed" : "failed";
        scenFailedDetails.push({
          step: step.name || "(undefined step)",
          error: "Step is undefined (no matching step definition)",
        });
      } else {
        // unknown/other -> count as skipped
        skippedSteps++;
      }
    }

    // Hooks (capture failures even if steps never ran)
    function absorbHookFailures(hooks, label) {
      for (const h of hooks) {
        const r = h.result || {};
        const s = (r.status || "unknown").toLowerCase();
        const durMs = nsToMsMaybe(r.duration);
        scenDurationMs += durMs;
        totalDurationMs += durMs;
        if (s === "failed") {
          scenStatus = "failed";
          scenFailedDetails.push({
            step: `(${label} hook)`,
            error: (r.error_message || r.error || "").toString() || `${label} hook failed`,
          });
        }
      }
    }
    absorbHookFailures(beforeHooks, "Before");
    absorbHookFailures(afterHooks, "After");

    // Scenario-level tallies
    totalScenarios++;
    featScenarios++;
    featDuration += scenDurationMs;

    if (scenStatus === "failed") {
      failedScenarios++;
      featFailed++;
      failures.push({ feature: featureName, scenario: scenarioName, steps: scenFailedDetails });
    } else if (scenStatus === "skipped") {
      skippedScenarios++;
      featSkipped++;
    } else {
      passedScenarios++;
      featPassed++;
    }

    scenarios.push({
      feature: featureName,
      name: scenarioName,
      status: scenStatus,
      durationMs: scenDurationMs,
    });
  }

  perFeature.set(featureName, {
    scenarios: featScenarios,
    passed: featPassed,
    failed: featFailed,
    skipped: featSkipped,
    durationMs: featDuration,
  });
}

// Markdown output
const overallStatus = failedScenarios > 0 ? `${cross} Failed` : `${tick} Passed`;
let md = `# 🥒 E2E Test Summary — ${overallStatus}\n\n`;

md += [
  `**Features:** ${totalFeatures}`,
  `**Scenarios:** ${passedScenarios}/${totalScenarios} passed`,
  `**Steps:** ${passedSteps} passed, ${failedSteps} failed, ${skippedSteps} skipped${pendingSteps ? `, ${pendingSteps} pending` : ""}${undefinedSteps ? `, ${undefinedSteps} undefined` : ""}`,
  `**Total Time:** ${msFmt(totalDurationMs)}`
].join("  •  ") + "\n\n";

md += `## 📦 Feature Overview\n`;
md += `| Feature | Scenarios | ${tick} Passed | ${cross} Failed | ${warn} Skipped | Time |\n`;
md += `|---|---:|---:|---:|---:|---:|\n`;
for (const [name, f] of perFeature.entries()) {
  md += `| ${name} | ${f.scenarios} | ${f.passed} | ${f.failed} | ${f.skipped} | ${msFmt(f.durationMs)} |\n`;
}
md += `\n`;

const slowest = [...scenarios].sort((a, b) => b.durationMs - a.durationMs).slice(0, Math.min(10, scenarios.length));
if (slowest.length) {
  md += `## 🐢 Slowest Scenarios\n`;
  md += `| # | Scenario | Feature | Status | Time |\n`;
  md += `|---:|---|---|:---:|---:|\n`;
  slowest.forEach((s, i) => {
    const badge = s.status === "failed" ? cross : (s.status === "skipped" ? warn : tick);
    md += `| ${i + 1} | ${s.name} | ${s.feature} | ${badge} ${s.status} | ${msFmt(s.durationMs)} |\n`;
  });
  md += `\n`;
}

if (failures.length) {
  md += `## ❌ Failed Scenarios\n`;
  failures.forEach((f) => {
    const header = `**${f.feature} → ${f.scenario}**`;
    md += `<details><summary>${header}</summary>\n\n`;
    f.steps.forEach(st => {
      const firstLine = (st.error || "").split("\n")[0];
      md += `- 🔴 **${st.step}**\n`;
      if (firstLine) md += `  - **Error:** ${firstLine}\n`;
    });
    md += `\n</details>\n\n`;
  });
}

md += `---\n*Report:* \`${path.resolve(input)}\`\n`;

if (outPath) {
  fs.appendFileSync(outPath, md);
  console.log("Wrote Cucumber summary to GITHUB_STEP_SUMMARY");
} else {
  console.log(md);
}

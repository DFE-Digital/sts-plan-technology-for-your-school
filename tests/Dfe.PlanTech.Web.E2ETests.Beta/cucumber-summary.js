// scripts/cucumber-summary.js
// Usage: node scripts/cucumber-summary.js ./tests/.../reports/cucumber-report.json

const fs = require("fs");
const path = require("path");

const input = process.argv[2] || "reports/cucumber-report.json";
const outPath = process.env.GITHUB_STEP_SUMMARY;

function nsToMs(ns) { return Number(ns || 0) / 1e6; }
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

function safeReadJSON(p) {
  try { return JSON.parse(fs.readFileSync(p, "utf8")); }
  catch (e) {
    console.error(`Failed to read JSON at ${p}:`, e.message);
    process.exit(1);
  }
}

const data = safeReadJSON(input);

// Aggregates
let totalFeatures = 0;
let totalScenarios = 0, passedScenarios = 0, failedScenarios = 0, skippedScenarios = 0;
let totalSteps = 0, passedSteps = 0, failedSteps = 0, skippedSteps = 0, pendingSteps = 0, undefinedSteps = 0;
let totalDurationMs = 0;

const perFeature = new Map(); // name -> { scenarios, passed, failed, skipped, durationMs }
const scenarios = []; // { feature, name, status, durationMs }
const failures = [];  // { feature, scenario, steps: [{step, error}] }

for (const feature of data) {
  const featureName = feature.name || "(unnamed feature)";
  totalFeatures++;

  const children = feature.elements || feature.children || [];
  let featDuration = 0, featScenarios = 0, featPassed = 0, featFailed = 0, featSkipped = 0;

  for (const el of children) {
    // Cucumber JSON can have backgrounds; skip those
    const type = (el.type || el.keyword || "").toString().toLowerCase();
    const isScenarioLike =
      (el.steps && el.name) && (type.includes("scenario") || type === "" || type === "rule");

    if (!isScenarioLike) continue;

    const scenarioName = el.name || "(unnamed scenario)";
    const steps = el.steps || [];
    let scenDurationMs = 0;
    let scenStatus = "passed";
    const scenFailedSteps = [];

    for (const step of steps) {
      const result = step.result || {};
      const status = (result.status || "unknown").toLowerCase();
      const durMs = nsToMs(result.duration);

      totalSteps++;
      totalDurationMs += durMs;
      scenDurationMs += durMs;

      if (status === "passed") passedSteps++;
      else if (status === "failed") {
        failedSteps++;
        scenStatus = "failed";
        scenFailedSteps.push({
          step: step.name || "(step)",
          error: (result.error_message || result.error || "").toString()
        });
      } else if (status === "skipped") {
        skippedSteps++;
      } else if (status === "pending") {
        pendingSteps++;
      } else if (status === "undefined") {
        undefinedSteps++;
      } else {
        // treat other/unknown as skipped for counts
        skippedSteps++;
      }
    }

    // Scenario status may be provided directly in newer JSONs
    if (el.tags && el.steps?.length === 0 && (el.status || el.result?.status)) {
      scenStatus = (el.status || el.result?.status).toLowerCase();
    }

    totalScenarios++;
    featScenarios++;
    featDuration += scenDurationMs;

    if (scenStatus === "failed") {
      failedScenarios++;
      featFailed++;
      failures.push({ feature: featureName, scenario: scenarioName, steps: scenFailedSteps });
    } else if (scenStatus === "skipped" || scenStatus === "pending") {
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
      durationMs: scenDurationMs
    });
  }

  perFeature.set(featureName, {
    scenarios: featScenarios,
    passed: featPassed,
    failed: featFailed,
    skipped: featSkipped,
    durationMs: featDuration
  });
}

// Build Markdown
let md = "";

// Header KPIs
const overallStatus = failedScenarios > 0 ? `${cross} Failed` : `${tick} Passed`;
md += `# 🥒 E2E Test Summary — ${overallStatus}\n\n`;

md += [
  `**Features:** ${totalFeatures}`,
  `**Scenarios:** ${passedScenarios}/${totalScenarios} passed`,
  `**Steps:** ${passedSteps} passed, ${failedSteps} failed, ${skippedSteps} skipped${pendingSteps ? `, ${pendingSteps} pending` : ""}${undefinedSteps ? `, ${undefinedSteps} undefined` : ""}`,
  `**Total Time:** ${msFmt(totalDurationMs)}`
].join("  •  ") + "\n\n";

// Per-feature table
md += `## 📦 Feature Overview\n`;
md += `| Feature | Scenarios | ${tick} Passed | ${cross} Failed | ${warn} Skipped | Time |\n`;
md += `|---|---:|---:|---:|---:|---:|\n`;
for (const [name, f] of perFeature.entries()) {
  md += `| ${name} | ${f.scenarios} | ${f.passed} | ${f.failed} | ${f.skipped} | ${msFmt(f.durationMs)} |\n`;
}
md += `\n`;

// Slowest scenarios
const slowest = [...scenarios]
  .sort((a, b) => b.durationMs - a.durationMs)
  .slice(0, Math.min(10, scenarios.length));

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

// Failures with collapsible details
if (failures.length) {
  md += `## ❌ Failed Scenarios\n`;
  failures.forEach((f, idx) => {
    const header = `**${f.feature} → ${f.scenario}**`;
    md += `<details><summary>${header}</summary>\n\n`;
    f.steps.forEach(st => {
      const firstLine = (st.error || "").split("\n")[0];
      md += `- 🔴 **Step:** \`${st.step}\`\n`;
      if (firstLine) md += `  - **Error:** ${firstLine}\n`;
    });
    md += `\n</details>\n\n`;
  });
}

// Where did the report come from
md += `---\n*Report:* \`${path.resolve(input)}\`\n`;

// Output
if (outPath) {
  fs.appendFileSync(outPath, md);
  console.log("Wrote Cucumber summary to GITHUB_STEP_SUMMARY");
} else {
  console.log(md);
}

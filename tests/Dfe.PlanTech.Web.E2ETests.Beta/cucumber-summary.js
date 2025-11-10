// scripts/cucumber-summary.js
// Usage: node scripts/cucumber-summary.js ./tests/.../reports/cucumber-report.json

const fs = require("fs");
const path = require("path");

const input = process.argv[2] || "reports/cucumber-report.json";
const outPath = process.env.GITHUB_STEP_SUMMARY;

// ---------- helpers ----------

// Prefer modern "children" (messages v19+) else legacy "elements"
function itemsOfFeature(feature) {
  if (Array.isArray(feature.children) && feature.children.length) return feature.children;
  return Array.isArray(feature.elements) ? feature.elements : [];
}

let DURATION_DIVISOR = 1;

function collectDurations(raw) {
  const vals = [];
  for (const feature of raw) {
    for (const el of itemsOfFeature(feature)) {
      const steps = Array.isArray(el.steps) ? el.steps : [];
      for (const s of steps) {
        const v = Number(s.result?.duration ?? 0);
        if (isFinite(v) && v > 0) vals.push(v);
      }
      const before = Array.isArray(el.before) ? el.before : [];
      const after  = Array.isArray(el.after)  ? el.after  : [];
      for (const h of [...before, ...after]) {
        const v = Number(h.result?.duration ?? 0);
        if (isFinite(v) && v > 0) vals.push(v);
      }
    }
  }
  vals.sort((a,b)=>a-b);
  return vals;
}

function setDurationDivisorFromData(raw) {
  const samples = collectDurations(raw);
  if (!samples.length) { DURATION_DIVISOR = 1; return; }
  const mid = samples[Math.floor(samples.length/2)];
  if (mid >= 10_000_000) { DURATION_DIVISOR = 1e6; return; } // ns -> ms
  if (mid >= 10_000)     { DURATION_DIVISOR = 1e3; return; } // µs -> ms
  DURATION_DIVISOR = 1;                                        // ms
}

// Normalize duration to milliseconds across ms / µs / ns
function toMs(value) {
  const v = Number(value || 0);
  if (!isFinite(v) || v <= 0) return 0;
  return v / DURATION_DIVISOR;
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
const warn  = "⚠️";

function readJSON(p) {
  return JSON.parse(fs.readFileSync(p, "utf8"));
}

function stepsOf(el) {
  return Array.isArray(el.steps) ? el.steps : [];
}

function hooksOf(el, key) {
  // key: "before" | "after" (legacy json puts hook results here)
  return Array.isArray(el[key]) ? el[key] : [];
}

function isScenarioLike(el) {
  // Skip backgrounds and rule containers
  const type = (el.type || el.keyword || "").toString().toLowerCase();
  if (type.includes("background") || type.includes("rule")) return false;
  // Consider anything with steps or a name a scenario-ish entry
  return !!(stepsOf(el).length || el.name);
}

// ---------- parse ----------

const data = readJSON(input);
setDurationDivisorFromData(data);

// Aggregates
let totalFeatures = 0;
let totalScenarios = 0, passedScenarios = 0, failedScenarios = 0, skippedScenarios = 0;
let totalSteps = 0, passedSteps = 0, failedSteps = 0, skippedSteps = 0, pendingSteps = 0, undefinedSteps = 0;
let totalDurationMs = 0;

const perFeature = new Map(); // name -> { scenarios, passed, failed, skipped, durationMs }
const scenarios = [];         // { feature, name, status, durationMs }
const failures = [];          // { feature, scenario, steps: [{ step, error }] }

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
    const afterHooks  = hooksOf(el, "after");

    let scenDurationMs = 0;
    let scenStatus = "passed";
    const scenFailedDetails = [];

    // Steps
    for (const step of stepList) {
      const result = step.result || {};
      const status = (result.status || "unknown").toLowerCase();
      const durMs = toMs(result.duration);

      totalSteps++;
      totalDurationMs += durMs;
      scenDurationMs += durMs;

      if (status === "passed") {
        passedSteps++;
      } else if (status === "failed") {
        failedSteps++;
        scenStatus = "failed";
        scenFailedDetails.push({
          step: step.name || "(step)",
          error: (result.error_message || result.error || "").toString(),
        });
      } else if (status === "skipped") {
        skippedSteps++;
        // keep scenario status as-is
      } else if (status === "pending") {
        pendingSteps++;
        scenStatus = "failed";
        scenFailedDetails.push({
          step: step.name || "(pending step)",
          error: "Step is pending",
        });
      } else if (status === "undefined") {
        undefinedSteps++;
        scenStatus = "failed";
        scenFailedDetails.push({
          step: step.name || "(undefined step)",
          error: "Step is undefined (no matching step definition)",
        });
      } else {
        // unknown/ambiguous -> treat as skipped for steps
        skippedSteps++;
      }
    }

    // Hooks (capture failures even if steps never ran)
    function absorbHookFailures(hooks, label) {
      for (const h of hooks) {
        const r = h.result || {};
        const s = (r.status || "unknown").toLowerCase();
        const durMs = toMs(r.duration);
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
    absorbHookFailures(afterHooks,  "After");

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

// ---------- markdown ----------

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

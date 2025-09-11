// scripts/cucumber-summary.js
// Usage: node scripts/cucumber-summary.js reports/cucumber-report.json

const fs = require("fs");

const input = process.argv[2] || "reports/cucumber-report.json";
const outPath = process.env.GITHUB_STEP_SUMMARY;

function nsToMs(ns) {
  return ns / 1e6;
}
function msFmt(ms) {
  if (ms >= 60_000) {
    const m = Math.floor(ms / 60_000);
    const s = Math.round((ms % 60_000) / 1000);
    return `${m}m ${s}s`;
  }
  if (ms >= 1000) return `${(ms / 1000).toFixed(2)}s`;
  return `${Math.round(ms)}ms`;
}

const data = JSON.parse(fs.readFileSync(input, "utf8"));

let totalSteps = 0,
  passedSteps = 0,
  failedSteps = 0,
  skippedSteps = 0;
let totalScenarios = 0,
  passedScenarios = 0,
  failedScenarios = 0;
let totalDurationNs = 0;

const scenarios = [];
const failedDetails = [];

for (const feature of data) {
  const featureName = feature.name || "(unnamed feature)";
  const elements = feature.elements || feature.children || [];

  for (const el of elements) {
    if ((el.type && el.type !== "scenario") && !el.steps) continue;

    const scenarioName = el.name || "(unnamed scenario)";
    const steps = el.steps || [];

    let scenDurationNs = 0;
    let scenFailed = false;
    const failedStepsList = [];

    for (const step of steps) {
      const result = step.result || {};
      const status = (result.status || "unknown").toLowerCase();
      const durNs = Number(result.duration || 0);

      totalSteps++;
      totalDurationNs += durNs;
      scenDurationNs += durNs;

      if (status === "passed") passedSteps++;
      else if (status === "failed") {
        failedSteps++;
        scenFailed = true;
        failedStepsList.push({
          step: step.name || "(step)",
          error: result.error_message || "(no error message)",
        });
      } else if (status === "skipped") skippedSteps++;
    }

    totalScenarios++;
    if (scenFailed) {
      failedScenarios++;
      failedDetails.push({
        feature: featureName,
        scenario: scenarioName,
        steps: failedStepsList,
      });
    } else {
      passedScenarios++;
    }

    scenarios.push({
      feature: featureName,
      name: scenarioName,
      durationNs: scenDurationNs,
      status: scenFailed ? "failed" : "passed",
    });
  }
}

// Markdown summary
let md = "";
md += `# 🥒 Cucumber Summary\n\n`;
md += `**Features:** ${data.length}  \n`;
md += `**Scenarios:** ${passedScenarios}/${totalScenarios} passed  \n`;
md += `**Steps:** ${passedSteps} passed, ${failedSteps} failed, ${skippedSteps} skipped  \n`;
md += `**Total Time:** ${msFmt(nsToMs(totalDurationNs))}\n\n`;

if (failedDetails.length > 0) {
  md += `## ❌ Failed Scenarios\n`;
  for (const scen of failedDetails) {
    md += `**${scen.feature} → ${scen.scenario}**\n`;
    for (const st of scen.steps) {
      md += `- 🔴 Step: \`${st.step}\`\n`;
      md += `  - Error: ${st.error.split("\n")[0]}\n`;
    }
    md += `\n`;
  }
}

if (outPath) {
  fs.appendFileSync(outPath, md);
  console.log("Wrote Cucumber summary to GITHUB_STEP_SUMMARY");
} else {
  console.log(md);
}

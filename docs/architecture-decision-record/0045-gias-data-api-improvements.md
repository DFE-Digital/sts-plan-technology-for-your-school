# 0045 – GIAS Data Refresh API Improvements

- **Status**: accepted

## Context and Problem Statement

ADR 0042 established the decision to implement automated GIAS data refresh using a proof-of-concept Python script running on a schedule via GitHub Actions. The original implementation used Playwright for web scraping to download GIAS data.

Following initial implementation, the proof-of-concept had significant limitations:

- Relied on Playwright web scraping, which was brittle and fragile to website changes
- Had minimal error handling and validation
- Could silently import corrupted or anomalous data into the database
- Lacked test coverage
- Was difficult to maintain and extend

These limitations created operational risk, particularly around accidental data corruption during routine runs.

## Decision Drivers

- Improve reliability and maintainability of the GIAS data refresh process
- Eliminate data corruption risk through comprehensive validation
- Reduce fragility caused by web scraping dependencies
- Provide visibility into data quality issues before they reach the database
- Make the implementation more testable and easier to extend

## Considered Options

- **Option 1**: Continue with Playwright-based scraping
  - Caveats:
    - Brittle to website changes
    - Difficult to debug when failures occur
    - No built-in error handling or validation
    - High maintenance burden as frameworks and dependencies evolve

- **Option 2**: Switch to public EDUBASE API with validation layer
  - Advantages:
    - Public EDUBASE API (`https://ea-edubase-api-prod.azurewebsites.net`) is mature and stable
    - API is more reliable than web scraping
    - Enables structured CSV data downloads for establishments, groups, and links
    - Allows building comprehensive validation layer
  - Caveats:
    - Requires code refactoring to replace Playwright
    - Need to design and implement validation rules

## Decision Outcome

We have chosen option 2: refactor the GIAS data refresh to use the public EDUBASE API with a comprehensive validation layer.

### Implementation Details

The improved implementation includes:

1. **API-based data fetching**: Downloads CSV files directly from the public EDUBASE API instead of relying on Playwright
2. **Validation framework**: Two-tier validation system:
   - **Structural validation**: Ensures required columns are populated, enforces foreign key integrity, catches data format issues
   - **Heuristic validation**: Verifies data meets expected row count thresholds and detects unexpectedly large changes that could indicate corrupted source data
3. **Skiippable heuristic checks**: Provides `--skip-validation` flag to disable heuristic checks when expecting unusual data patterns (e.g., term changeover, known GIAS reorganization), while always enforcing structural checks
4. **Expanded data fetching**: Now fetches all three key GIAS datasets (establishments, groups, links) via the API

### Benefits

- **Reliability**: Using the public API eliminates web scraping fragility
- **Data safety**: Comprehensive validation prevents corrupted data from reaching the database
- **Maintainability**: Cleaner, more testable codebase with proper error handling
- **Visibility**: Clear logging and validation messages provide insight into data quality
- **Flexibility**: Validation can be disabled when appropriate, but structural checks always run

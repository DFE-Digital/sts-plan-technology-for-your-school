# 0020 - GitHub Actions Locations

- **Status**: accepted

## Context and Problem Statement

Where do we store our GitHub action workflow files, and how do we manage these?

## Decision Drivers

- Within DfE’s Technical Guidance
- Developer Readability; is it easy to understand/find/follow where each workflow file is?

## Considered Options

- All pipeline files would be in the folder `./pipelines`
- All pipeline files would be in the folder `./.github/workflows/`
- Pipeline files would be in their own separate project folders, e.g. `./src/project-1/pipelines/` and `./src/project-2/pipelines`

## Pros and Cons of the Options

### ./pipelines folder

- Good; keeps the project clean, as all pipeline files are in one place.
- Good; it keeps the project easy to read. It should be immediately obvious to a new developer, looking at the repository, where the pipeline files are.

- Bad; functionally, it is not possible to store _all_ workflow files in that directory. [GitHub will only detect files in the ./github/workflows/ folder](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions).
  - However, this could be managed by using a parent workflow, in the `./.github/workflows/` directory, that includes the various other pipeline files in a `./pipelines/` folder. For an example, please see this [GitHub discussion](https://github.com/orgs/community/discussions/18055#discussioncomment-5793379)

### ./github/workflows

- Good; keeps the project clean, as all pipeline files are in one place.
- Good; this is the way GitHub actions work, so anyone who is familiar with GitHub would understand where they are.

- Bad; it does not match our ideal convention of `./pipelines`
- Bad; anyone who _isn't_ experienced with GitHub might not be able to find these immediately
  - This could be managed by ensuring our documentation is correct, up-to-date, readable, and ensuring it includes information as to where the workflows are.

### Seperate folders per project

- Good; keeps separation of concerns for projects
- Good; if projects are removed/moved from this repository (i.e. we change from a monorepo, to multiple repos), then we would not have to move workflows around

- Bad; all the same negatives as the .`/pipelines` folder - i.e. this is not natively supported by GitHub

## Decision Outcome

GitHub Workflows must reside within the `.github/workflows` directory as described within the article [Workflow syntax for GitHub Actions](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions).

Also it is advised that the reusable Actions are located within the `.github/actions` directory as discussed within the [About custom actions](https://docs.github.com/en/actions/creating-actions/about-custom-actions#choosing-a-location-for-your-action) article.

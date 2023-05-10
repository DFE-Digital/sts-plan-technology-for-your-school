# 0003 - Build/Test/Deploy Environments

* **Status**: Accepted

## Context and Problem Statement

How to ensure consistency during application build, testing and deployment across varying hosts and operating systems?

## Decision Drivers

* Faster setup during onboarding for new Developers
* Parity of development environment for each Developer
* Eliminate dependencies on system level libraries
* Portability to easily deploy application on different hosts
* Solution prominent and widely used within industry

## Considered Options

* Containerisation with Docker
* Native framework/manual installation

## Decision Outcome

Chosen option: Containerised environment using [Docker](https://www.docker.com/).
---
type: adr
title: Infrastructure As Code
created: 2022-11-16
status: Accepted
---

# Infrastructure As Code

* **Status**: Accepted

## Context and Problem Statement

How can we ensure that hosting environments are configured consistently and track changes made?

## Decision Drivers

* Open source as per Service Standard
* Compatible with selected hosting platform
* Infrastructure changes are scripted, versioned and reversible
* Portability
* Prominent within industry

## Considered Options

* Terraform
* ARM Templates 

## Decision Outcome

Chosen option: [Terraform](https://www.terraform.io) as this is open source.
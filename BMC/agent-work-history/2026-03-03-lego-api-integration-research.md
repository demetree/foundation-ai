# LEGO Community API Integration Research

**Date:** 2026-03-03

## Summary

Explored the existing Rebrickable and BrickSet integrations in BMC.Server and BMC.Client to understand the established patterns, then conducted comprehensive internet research to identify all LEGO community services with public APIs that could be integrated into the BMC platform.

## Changes Made

- Created `.agents/workflows/onboarding.md` — onboarding workflow for new AI agents/developers
- Created `.agents/workflows/save-work-history.md` — workflow for saving session artifacts

## Key Decisions

- Identified **BrickLink** as the #1 priority new integration — the largest LEGO marketplace with the only source of accurate secondary market pricing at the part level
- Identified **BrickEconomy** as #2 priority — AI-powered investment valuation API with future price predictions
- Identified **Brick Owl** as #3 — second largest marketplace with generous rate limits (600 req/min)
- All new integrations should follow the established pattern: separate class library (`BMC.{Service}/Api/` + `BMC.{Service}/Sync/`), server controller, Angular service

## Research Output

- Full research report saved as a session artifact at `brain/212f4885-592c-4d2f-8f48-2f07d04e7de9/lego-api-integration-research.md`
- Covered 6+ services: BrickLink, BrickEconomy, Brick Owl, BrickScout, Brickwatch, cubiculus, LDraw, LEGO.com internal APIs

## Testing / Verification

- N/A — research-only session, no code changes to test

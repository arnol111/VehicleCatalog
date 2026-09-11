# car-model-list Specification

## Purpose

Defines requirements for the `GET /api/carModels` endpoint with optional brand-name filtering.

## Requirements

### Requirement: List All Car Models

The system MUST expose `GET /api/carModels` and return all models with their brand name and year.

#### Scenario: Models exist, no filter

- GIVEN the database contains car models
- WHEN a client sends `GET /api/carModels` with no query parameters
- THEN the response status is 200 OK
- AND the body is a JSON array of `{ id, name, year, brandName }`

#### Scenario: No models in database

- GIVEN the database has no car model records
- WHEN a client sends `GET /api/carModels`
- THEN the response status is 200 OK
- AND the body is an empty JSON array `[]`

### Requirement: Filter Car Models by Brand Name

The system MUST support filtering models via `?Brand={name}`. The match MUST be case-insensitive.

#### Scenario: Filter returns matching models

- GIVEN car models exist for brand "Toyota"
- WHEN a client sends `GET /api/carModels?Brand=toyota`
- THEN the response status is 200 OK
- AND the body contains only models whose brand name matches "Toyota" (case-insensitive)

#### Scenario: Filter returns no matches for valid brand with no models

- GIVEN brand "EmptyBrand" exists but has no associated models
- WHEN a client sends `GET /api/carModels?Brand=EmptyBrand`
- THEN the response status is 200 OK
- AND the body is an empty JSON array `[]`

#### Scenario: Brand param is empty string

- GIVEN the query parameter `Brand` is present but empty (`?Brand=`)
- WHEN a client sends `GET /api/carModels?Brand=`
- THEN the response status is 400 Bad Request

#### Scenario: Brand name does not exist

- GIVEN no brand matching the provided name exists in the database
- WHEN a client sends `GET /api/carModels?Brand=NonExistent`
- THEN the response status is 404 Not Found

#### Scenario: Database error

- GIVEN an unexpected database failure occurs
- WHEN a client sends `GET /api/carModels`
- THEN the response status is 500 Internal Server Error

---

# car-brand-list Specification

## Purpose

Defines requirements for the `GET /api/carBrands` endpoint that returns all car brands.

## Requirements

### Requirement: List All Car Brands

The system MUST expose `GET /api/carBrands` and return all car brands persisted in the database.

#### Scenario: Brands exist

- GIVEN the database contains one or more car brands
- WHEN a client sends `GET /api/carBrands`
- THEN the response status is 200 OK
- AND the body is a JSON array of objects with `id` and `name` fields

#### Scenario: No brands in database

- GIVEN the database has no car brand records
- WHEN a client sends `GET /api/carBrands`
- THEN the response status is 200 OK
- AND the body is an empty JSON array `[]`

#### Scenario: Database error

- GIVEN an unexpected database failure occurs
- WHEN a client sends `GET /api/carBrands`
- THEN the response status is 500 Internal Server Error

---

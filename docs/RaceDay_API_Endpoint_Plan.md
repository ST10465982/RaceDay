# RaceDay API Endpoint Plan

This document defines the RESTful API endpoints that will be implemented for the RaceDay system in Part 2 of the Portfolio of Evidence.

The API supports two system roles:

- **Organiser** – creates, updates and deletes events, manages categories, views event enrolments and captures participant results.
- **Participant** – creates an account, manages their profile, browses events, enters events, views their enrolments and tracks personal results.

Role-based access will be enforced at API level.
## API Endpoint Specification

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Creates a new RaceDay account as an Organiser or Participant. | None | `{ "firstName": "string", "lastName": "string", "email": "string", "password": "string", "role": "Organiser or Participant" }` | **201 Created** – account created. **400 Bad Request** – invalid data. **409 Conflict** – email already exists. |
| POST | `/api/auth/login` | Authenticates a registered user and returns authentication details. | None | `{ "email": "string", "password": "string" }` | **200 OK** – login successful. **400 Bad Request** – missing data. **401 Unauthorized** – incorrect credentials. |
| GET | `/api/profile` | Retrieves the profile of the currently logged-in user. | Any | None | **200 OK** – profile returned. **401 Unauthorized** – user is not logged in. **404 Not Found** – profile does not exist. |
| POST | `/api/profile` | Creates a profile for the currently logged-in user. | Any | `{ "phoneNumber": "string", "dateOfBirth": "date", "emergencyContactName": "string", "emergencyContactNumber": "string" }` | **201 Created** – profile created. **400 Bad Request** – invalid data. **401 Unauthorized** – user is not logged in. **409 Conflict** – profile already exists. |
| PUT | `/api/profile` | Updates the profile information of the currently logged-in user. | Any | `{ "phoneNumber": "string", "dateOfBirth": "date", "emergencyContactName": "string", "emergencyContactNumber": "string" }` | **200 OK** – profile updated. **400 Bad Request** – invalid data. **401 Unauthorized** – user is not logged in. **404 Not Found** – profile does not exist. |
| GET | `/api/events` | Retrieves available RaceDay events for browsing. | None | None | **200 OK** – event list returned. |
| GET | `/api/events/{id}` | Retrieves the details of a specific event. | None | None | **200 OK** – event returned. **404 Not Found** – event does not exist. |
| POST | `/api/events` | Creates a new RaceDay event. | Organiser | `{ "categoryId": 1, "eventName": "string", "description": "string", "eventDate": "date", "startTime": "time", "location": "string", "distanceKm": 10.0, "maximumParticipants": 100, "status": "Open" }` | **201 Created** – event created. **400 Bad Request** – invalid event data. **401 Unauthorized** – user is not logged in. **403 Forbidden** – user is not an Organiser. **404 Not Found** – category does not exist. |
| PUT | `/api/events/{id}` | Updates an existing event created by an Organiser. | Organiser | `{ "categoryId": 1, "eventName": "string", "description": "string", "eventDate": "date", "startTime": "time", "location": "string", "distanceKm": 10.0, "maximumParticipants": 100, "status": "Open" }` | **200 OK** – event updated. **400 Bad Request** – invalid data. **401 Unauthorized** – user is not logged in. **403 Forbidden** – user is not authorised to update the event. **404 Not Found** – event does not exist. |
| DELETE | `/api/events/{id}` | Deletes an existing event. | Organiser | None | **204 No Content** – event deleted. **401 Unauthorized** – user is not logged in. **403 Forbidden** – user is not authorised to delete the event. **404 Not Found** – event does not exist. |
| GET | `/api/categories` | Retrieves all RaceDay event categories. | None | None | **200 OK** – category list returned. |
| GET | `/api/categories/{id}` | Retrieves a specific event category. | None | None | **200 OK** – category returned. **404 Not Found** – category does not exist. |
| POST | `/api/categories` | Creates a new event category. | Organiser | `{ "categoryName": "string", "description": "string" }` | **201 Created** – category created. **400 Bad Request** – invalid data. **401 Unauthorized** – user is not logged in. **403 Forbidden** – user is not an Organiser. **409 Conflict** – category name already exists. |
| PUT | `/api/categories/{id}` | Updates an existing event category. | Organiser | `{ "categoryName": "string", "description": "string" }` | **200 OK** – category updated. **400 Bad Request** – invalid data. **401 Unauthorized** – user is not logged in. **403 Forbidden** – user is not an Organiser. **404 Not Found** – category does not exist. **409 Conflict** – category name already exists. |
| DELETE | `/api/categories/{id}` | Deletes an event category where deletion is permitted. | Organiser | None | **204 No Content** – category deleted. **401 Unauthorized** – user is not logged in. **403 Forbidden** – user is not an Organiser. **404 Not Found** – category does not exist. **409 Conflict** – category is currently assigned to an event. |
| POST | `/api/events/{eventId}/enrollments` | Enrols the logged-in Participant in an event. | Participant | None | **201 Created** – enrollment created. **400 Bad Request** – event is closed or full. **401 Unauthorized** – user is not logged in. **403 Forbidden** – user is not a Participant. **404 Not Found** – event does not exist. **409 Conflict** – participant is already enrolled. |
| GET | `/api/enrollments/me` | Retrieves all event enrollments belonging to the logged-in Participant. | Participant | None | **200 OK** – enrollment list returned. **401 Unauthorized** – user is not logged in. **403 Forbidden** – user is not a Participant. |
| GET | `/api/enrollments/{id}` | Retrieves the details of one of the logged-in Participant's enrollments. | Participant | None | **200 OK** – enrollment returned. **401 Unauthorized** – user is not logged in. **403 Forbidden** – enrollment does not belong to the Participant. **404 Not Found** – enrollment does not exist. |
| DELETE | `/api/enrollments/{id}` | Cancels an event enrollment belonging to the logged-in Participant. | Participant | None | **204 No Content** – enrollment cancelled. **400 Bad Request** – enrollment can no longer be cancelled. **401 Unauthorized** – user is not logged in. **403 Forbidden** – enrollment does not belong to the Participant. **404 Not Found** – enrollment does not exist. |
| GET | `/api/events/{eventId}/enrollments` | Retrieves participants enrolled in a specific event for management purposes. | Organiser | None | **200 OK** – enrollment list returned. **401 Unauthorized** – user is not logged in. **403 Forbidden** – user is not authorised to manage the event. **404 Not Found** – event does not exist. |
| POST | `/api/enrollments/{enrollmentId}/result` | Captures a result for a completed participant enrollment. | Organiser | `{ "finishTime": "time", "overallPosition": 1, "categoryPosition": 1 }` | **201 Created** – result recorded. **400 Bad Request** – invalid result information. **401 Unauthorized** – user is not logged in. **403 Forbidden** – user is not authorised to manage the event. **404 Not Found** – enrollment does not exist. **409 Conflict** – result already exists for the enrollment. |
| PUT | `/api/results/{id}` | Updates an incorrectly captured participant result. | Organiser | `{ "finishTime": "time", "overallPosition": 1, "categoryPosition": 1 }` | **200 OK** – result updated. **400 Bad Request** – invalid result information. **401 Unauthorized** – user is not logged in. **403 Forbidden** – user is not authorised to manage the result. **404 Not Found** – result does not exist. |
| GET | `/api/events/{eventId}/results` | Retrieves recorded results for a specific event. | None | None | **200 OK** – event results returned. **404 Not Found** – event does not exist. |
| GET | `/api/results/me` | Retrieves the personal race results of the logged-in Participant. | Participant | None | **200 OK** – personal results returned. **401 Unauthorized** – user is not logged in. **403 Forbidden** – user is not a Participant. |
| GET | `/api/results/{id}` | Retrieves a specific recorded result. | Any | None | **200 OK** – result returned. **401 Unauthorized** – user is not logged in. **404 Not Found** – result does not exist. |

## Role-Based Access Summary

RaceDay uses role-based access control to protect functionality at API level. Public endpoints such as event browsing do not require authentication. Authenticated users can manage their own profiles while functionality that changes events, categories and participant results is restricted to Organisers. Event entry and personal enrollment functionality is restricted to Participants.

The Organiser associated with an event will also be checked when event-specific management operations are performed. This prevents one Organiser from modifying another Organiser's events, enrollments or results.

## Design Notes

The endpoint plan corresponds with the RaceDay ERD. User accounts are represented by the USERS entity while the Role field distinguishes Organisers from Participants. Event enrollment is represented by the ENROLLMENTS entity and a Participant cannot enrol in the same event more than once. Each enrollment can have a maximum of one recorded result.

The implemented RESTful API in Part 2 should follow this plan closely. Any necessary implementation changes should be documented in the project README.

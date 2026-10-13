# RaceDay POE - API Endpoint Plan
This document lists all planned endpoints for the RaceDay system.  
It covers Authentication, User Profile, Events, Categories, Enrolments, and Results.  
Each endpoint includes method, route, description, role required, request body, and expected response.

---

## Authentication
| HTTP Method | Route              | Description                       | Role Required | Request Body                       | Expected Response |
|-------------|--------------------|-----------------------------------|---------------|------------------------------------|------------------|
| POST        | /api/auth/register | Register a new user (participant or organiser) | None          | { username, password, role }       | 201 Created / 400 Bad Request |
| POST        | /api/auth/login    | Authenticate user and return JWT token | None          | { username, password }             | 200 OK / 401 Unauthorized |

---

## User Profile
| HTTP Method | Route             | Description              | Role Required | Request Body | Expected Response |
|-------------|-------------------|--------------------------|---------------|--------------|------------------|
| GET         | /api/users/{id}   | Get user profile         | Any           | None         | 200 OK / 404 Not Found |
| PUT         | /api/users/{id}   | Update user profile      | Any           | { name, email, password } | 200 OK / 400 Bad Request / 404 Not Found |

---

## Events
| HTTP Method | Route             | Description              | Role Required | Request Body | Expected Response |
|-------------|-------------------|--------------------------|---------------|--------------|------------------|
| POST        | /api/events       | Create a new event       | Organiser     | { name, date, location, description } | 201 Created / 400 Bad Request |
| GET         | /api/events       | List all events          | Any           | None         | 200 OK |
| GET         | /api/events/{id}  | Get event details        | Any           | None         | 200 OK / 404 Not Found |
| PUT         | /api/events/{id}  | Update event details     | Organiser     | { name, date, location, description } | 200 OK / 404 Not Found |
| DELETE      | /api/events/{id}  | Delete an event          | Organiser     | None         | 204 No Content / 404 Not Found |

---

## Categories
| HTTP Method | Route                     | Description                  | Role Required | Request Body | Expected Response |
|-------------|---------------------------|------------------------------|---------------|--------------|------------------|
| POST        | /api/events/{id}/categories | Add category to an event   | Organiser     | { name, distance } | 201 Created / 404 Not Found |
| GET         | /api/events/{id}/categories | List categories for an event | Any           | None         | 200 OK / 404 Not Found |

---

## Enrolments
| HTTP Method | Route                        | Description                        | Role Required | Request Body | Expected Response |
|-------------|------------------------------|------------------------------------|---------------|--------------|------------------|
| POST        | /api/events/{id}/enrol       | Enrol participant in event category | Participant   | { categoryId } | 201 Created / 404 Not Found / 409 Conflict |
| GET         | /api/users/{id}/enrolments   | View participant’s enrolments       | Participant   | None         | 200 OK / 404 Not Found |

---

## Results
| HTTP Method | Route                        | Description                        | Role Required | Request Body | Expected Response |
|-------------|------------------------------|------------------------------------|---------------|--------------|------------------|
| POST        | /api/events/{id}/results     | Capture participant results         | Organiser     | { participantId, time, position } | 201 Created / 404 Not Found |
| GET         | /api/users/{id}/results      | View participant’s results history  | Participant   | None         | 200 OK / 404 Not Found |
| GET         | /api/events/{id}/results     | View all results for an event       | Any           | None         | 200 OK / 404 Not Found |


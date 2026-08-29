# RaceDay POE - API Endpoint Plan

## Base URL
`https://api.raceday.example.com/v1`

## Authentication Endpoints

### User Login
- **Endpoint**: `POST /auth/login`
- **Description**: Authenticate user and get access token
- **Request Body**:
  ```json
  {
    "email": "user@example.com",
    "password": "password123"
  }
  ```
- **Response**: `200 OK`
  ```json
  {
    "access_token": "jwt_token",
    "user_id": 1
  }
  ```

### User Registration
- **Endpoint**: `POST /auth/register`
- **Description**: Create new user account
- **Request Body**:
  ```json
  {
    "username": "john_doe",
    "email": "john@example.com",
    "password": "password123"
  }
  ```
- **Response**: `201 Created`

### User Logout
- **Endpoint**: `POST /auth/logout`
- **Description**: Logout user session
- **Response**: `200 OK`

## Race Endpoints

### Get All Races
- **Endpoint**: `GET /races`
- **Description**: Retrieve list of all races
- **Response**: `200 OK` (array of race objects)

### Get Race Details
- **Endpoint**: `GET /races/{race_id}`
- **Description**: Get specific race information
- **Response**: `200 OK`

### Create Race
- **Endpoint**: `POST /races`
- **Description**: Create new race event
- **Request Body**:
  ```json
  {
    "name": "Marathon 2026",
    "date": "2026-09-15",
    "location": "Central Park"
  }
  ```
- **Response**: `201 Created`

## User Endpoints

### Get User Profile
- **Endpoint**: `GET /users/{user_id}`
- **Description**: Get user profile information
- **Response**: `200 OK`

### Update User Profile
- **Endpoint**: `PUT /users/{user_id}`
- **Description**: Update user information
- **Response**: `200 OK`

## Error Responses
All endpoints return standard error responses:
- `400 Bad Request`: Invalid input
- `401 Unauthorized`: Authentication required
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

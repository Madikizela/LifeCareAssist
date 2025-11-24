# API Documentation

## Base URL
`https://localhost:5001/api`

## Endpoints

### Get Patient Medications
```
GET /api/patients/{id}/medications
```
Returns all active medications for a patient.

**Response:**
```json
[
  {
    "id": "uuid",
    "name": "Metformin",
    "dosage": "500mg",
    "frequency": "twice_daily",
    "reminderTimes": ["08:00", "20:00"],
    "instructions": "Take with food"
  }
]
```

### Log Medication Taken
```
POST /api/medications/{id}/log
```
Records that a medication was taken.

**Request Body:**
```json
{
  "scheduledTime": "2024-01-15T08:00:00Z",
  "notes": "Taken with breakfast"
}
```

### Get Active Emergencies
```
GET /api/emergency/active
```
Returns all pending or dispatched emergency calls.

**Response:**
```json
[
  {
    "id": "uuid",
    "emergencyType": "medical",
    "callTime": "2024-01-15T10:30:00Z",
    "status": "pending",
    "latitude": -26.2041,
    "longitude": 28.0473,
    "patient": {
      "firstName": "John",
      "lastName": "Doe",
      "phoneNumber": "+27123456789"
    }
  }
]
```

## Future Endpoints

### USSD Integration
- `/api/ussd/menu` - USSD menu handler
- `/api/ussd/medication-reminder` - Check medication schedule via USSD

### SMS Webhooks
- `/api/sms/incoming` - Handle incoming SMS responses
- `/api/sms/delivery-status` - Track SMS delivery

### Mobile App
- `/api/auth/login` - User authentication
- `/api/patients/profile` - Patient profile management
- `/api/appointments/upcoming` - Get upcoming appointments

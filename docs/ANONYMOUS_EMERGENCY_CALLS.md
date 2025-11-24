# Anonymous Emergency Calls

## Overview

The Rural Healthcare Platform now supports emergency calls from **unregistered/anonymous callers**. This is critical for walk-in emergencies or situations where someone needs immediate help but isn't registered in the system.

## How It Works

### Creating an Emergency Call

When creating an emergency call, users can choose between:

1. **Registered Patient** - Select from existing patients in the system
2. **Anonymous/Walk-in** - Provide basic caller information

### Anonymous Caller Information

For anonymous calls, the following information is captured:

- **Caller Name** (Required) - Full name of the person calling
- **Phone Number** (Required) - Contact number for follow-up
- **ID Number** (Optional) - South African ID if available
- **Emergency Type** - Medical or Security
- **Description** - Details about the emergency
- **Location Description** - Landmarks or address
- **GPS Coordinates** - If available

## Benefits

### Immediate Response
- No need to register a patient before calling for help
- Faster emergency response times
- Reduces barriers to accessing emergency services

### Patient Record Creation
- After the emergency is resolved, staff can create a patient record
- Anonymous caller information can be used to populate the new patient record
- Links the emergency call to the newly created patient

### Data Tracking
- All emergency calls are tracked, even for non-registered individuals
- Helps identify areas with high emergency call volumes
- Supports resource planning and allocation

## User Interface

### Emergency Call Form

The form includes a toggle to switch between:
- **Registered Patient** mode (default)
- **Anonymous/Walk-in** mode

When Anonymous mode is selected:
- Patient dropdown is hidden
- Caller information fields appear
- All required fields must be filled

### Emergency List View

Anonymous emergency calls are displayed with:
- Yellow "Anonymous/Walk-in" badge
- Caller name and phone number
- "Create Patient Record" button instead of "View Patient"

## Workflow

### For Emergency Responders

1. **Receive Call** - Emergency call comes in (registered or anonymous)
2. **Dispatch** - Send ambulance/emergency services
3. **Respond** - Mark as arrived when on scene
4. **Complete** - Mark as completed when resolved
5. **Follow-up** - If anonymous, create patient record for future care

### For Administrative Staff

1. **Review Anonymous Calls** - Check emergency calls list
2. **Create Patient Record** - Click "Create Patient Record" button
3. **Link Records** - System can link the emergency call to new patient
4. **Update Information** - Add medical history, chronic conditions, etc.

## Database Schema

### EmergencyCall Entity

```csharp
public class EmergencyCall
{
    public Guid Id { get; set; }
    public Guid? PatientId { get; set; } // Nullable for anonymous calls
    
    // Anonymous caller information
    public string? CallerName { get; set; }
    public string? CallerPhone { get; set; }
    public string? CallerIdNumber { get; set; }
    
    public string EmergencyType { get; set; }
    public DateTime CallTime { get; set; }
    public string? LocationDescription { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Status { get; set; }
    public string? Description { get; set; }
    
    public Patient? Patient { get; set; }
}
```

## Security Considerations

### Data Privacy
- Anonymous caller information is stored securely
- Only authorized staff can view emergency call details
- Phone numbers are indexed for quick lookup

### Access Control
- System Admins can view all emergency calls
- Clinic Admins can view calls from their area
- Health Workers can create and view emergency calls
- Caregivers can create emergency calls

## Future Enhancements

- [ ] Automatic patient matching based on phone number or ID
- [ ] SMS notifications to caller with ambulance ETA
- [ ] Integration with external emergency services (10177)
- [ ] Voice call recording for quality assurance
- [ ] Real-time ambulance tracking
- [ ] Post-emergency follow-up reminders

## Testing

### Test Anonymous Emergency Call

1. Navigate to Emergency → New Emergency Call
2. Select "Anonymous/Walk-in" option
3. Fill in:
   - Caller Name: "John Doe"
   - Phone: "+27821234567"
   - Emergency Type: Medical
   - Description: "Chest pain"
   - Location: "Corner of Main St and 5th Ave"
4. Click "Dispatch Emergency Services"
5. Verify call appears in Emergency list with "Anonymous" badge

### Test Patient Record Creation

1. Go to Emergency list
2. Find an anonymous emergency call
3. Click "Create Patient Record"
4. System should pre-fill name and phone from emergency call
5. Complete patient registration
6. Emergency call should now link to the new patient

## Support

For questions or issues with anonymous emergency calls, contact:
- System Administrator
- Technical Support Team

---

**Last Updated:** November 23, 2025

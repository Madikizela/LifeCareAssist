# Emergency Dispatch Workflow

## Overview

The Rural Healthcare Platform includes a complete emergency dispatch workflow that allows authorized staff to manage ambulance requests from initial call to completion.

## Who Can Dispatch Ambulances?

### Authorized Roles

1. **System Admin** ✅
   - Can dispatch any emergency call
   - Full access to all emergency management features

2. **Clinic Admin** ✅
   - Can dispatch ambulances for their clinic area
   - Manages emergencies in their jurisdiction

3. **Health Worker** ✅
   - Can dispatch ambulances
   - Front-line staff who receive emergency calls

4. **Caregiver** ❌
   - Cannot dispatch (can only create emergency calls)

## Emergency Call Lifecycle

### 1. PENDING (Red)
**Initial State** - Emergency call has been created but not yet dispatched

**Actions Available:**
- 🚨 **Dispatch Ambulance** button (visible to authorized users)

**What Happens:**
- Call appears in the Active Emergencies list
- Shows caller information (registered patient or anonymous)
- Displays emergency type, location, and description
- Timer shows how long the call has been waiting

### 2. DISPATCHED (Yellow)
**Ambulance En Route** - Emergency services have been notified and are on the way

**Actions Available:**
- 📍 **Mark Arrived** button

**What Happens:**
- Status changes to "dispatched"
- DispatchedAt timestamp is recorded
- Shows "Dispatched X min ago" indicator
- TODO: SMS sent to caller with ETA
- TODO: Ambulance driver notified

### 3. ARRIVED (Blue)
**On Scene** - Ambulance has arrived at the location

**Actions Available:**
- ✅ **Complete** button

**What Happens:**
- Status changes to "arrived"
- ArrivedAt timestamp is recorded
- Shows "Arrived X min ago" indicator
- Emergency responders are providing care

### 4. COMPLETED (Green)
**Resolved** - Emergency has been handled and patient is safe

**What Happens:**
- Status changes to "completed"
- CompletedAt timestamp is recorded
- Call is removed from Active Emergencies list
- Available in historical records for reporting

## User Interface

### Emergency List View

Each emergency call card displays:

**Header:**
- Patient/Caller name and phone
- Status badge (color-coded)
- Anonymous badge (if applicable)

**Details:**
- Emergency type (Medical 🚑 or Security 👮)
- Call time
- Location (GPS or description)
- Duration since call was received
- Description of emergency

**Actions:**
- Status-specific action buttons
- View Patient / Create Patient Record
- Timestamps for dispatch, arrival, completion

### Button States

```
PENDING    → [🚨 Dispatch Ambulance]
DISPATCHED → [📍 Mark Arrived] + "Dispatched 5min ago"
ARRIVED    → [✅ Complete] + "Arrived 10min ago"
COMPLETED  → (Removed from active list)
```

## Workflow Example

### Scenario: Medical Emergency for Anonymous Caller

1. **Call Received (00:00)**
   - Health worker receives call from unknown person
   - Creates emergency call as "Anonymous/Walk-in"
   - Enters: Name, Phone, Location, Description
   - Status: PENDING

2. **Dispatch (00:02)**
   - Clinic admin reviews the call
   - Clicks "🚨 Dispatch Ambulance"
   - System records dispatch time
   - Status: DISPATCHED
   - SMS sent to caller: "Ambulance dispatched, ETA 15 minutes"

3. **Arrival (00:17)**
   - Ambulance driver arrives on scene
   - Health worker clicks "📍 Mark Arrived"
   - System records arrival time
   - Status: ARRIVED

4. **Treatment & Transport (00:17 - 00:45)**
   - Paramedics provide emergency care
   - Patient stabilized and transported to hospital

5. **Completion (00:45)**
   - Health worker clicks "✅ Complete"
   - System records completion time
   - Status: COMPLETED
   - Call removed from active list

6. **Follow-up (Later)**
   - Admin clicks "Create Patient Record"
   - Registers patient in system using emergency call data
   - Links emergency call to new patient record

## Permissions & Security

### Authorization Checks

```csharp
// Only these roles can dispatch
if (userRole != "system_admin" && 
    userRole != "clinic_admin" && 
    userRole != "health_worker")
{
    return AccessDenied;
}
```

### Data Filtering

- System Admins see ALL emergency calls
- Clinic Admins see calls from their area (TODO: implement filtering)
- Health Workers see calls they can respond to

### Audit Trail

All status changes are timestamped:
- `CallTime` - When emergency was reported
- `DispatchedAt` - When ambulance was dispatched
- `ArrivedAt` - When ambulance arrived on scene
- `CompletedAt` - When emergency was resolved

## Notifications (TODO)

### Planned Notifications

1. **On Dispatch:**
   - SMS to caller: "Ambulance dispatched, ETA X minutes"
   - Notification to ambulance driver with location
   - Alert to clinic admin

2. **On Arrival:**
   - SMS to caller: "Ambulance has arrived"
   - Update to clinic dashboard

3. **On Completion:**
   - SMS to caller: "Emergency resolved, thank you"
   - Update statistics and reports

## Reporting & Analytics

### Metrics Tracked

- Average response time (Call → Dispatch)
- Average arrival time (Dispatch → Arrival)
- Average completion time (Call → Completion)
- Emergency calls by type (Medical vs Security)
- Anonymous vs Registered patient calls
- Peak emergency hours
- Geographic hotspots

### Dashboard Widgets (TODO)

- Active emergencies count
- Average response time today
- Emergencies by status
- Recent completions

## Integration Points

### Current
- Database tracking of all status changes
- Session-based authentication
- Role-based access control

### Future
- SMS gateway integration (Twilio, Africa's Talking)
- GPS tracking for ambulances
- Real-time map view of active emergencies
- Integration with 10177 (South African emergency number)
- Voice call recording
- WhatsApp notifications

## Testing

### Test Dispatch Workflow

1. **Create Emergency:**
   - Login as health worker
   - Go to Emergency → New Emergency
   - Create anonymous emergency call

2. **Dispatch:**
   - Should see "🚨 Dispatch Ambulance" button
   - Click to dispatch
   - Verify status changes to DISPATCHED
   - Check timestamp is recorded

3. **Mark Arrived:**
   - Click "📍 Mark Arrived"
   - Verify status changes to ARRIVED
   - Check arrival timestamp

4. **Complete:**
   - Click "✅ Complete"
   - Verify status changes to COMPLETED
   - Confirm call removed from active list

### Test Permissions

1. **As Caregiver:**
   - Can create emergency calls
   - Cannot see dispatch button (should show error)

2. **As Health Worker:**
   - Can create and dispatch emergency calls
   - Can mark arrived and complete

3. **As Clinic Admin:**
   - Can dispatch ambulances
   - Can manage all emergency calls in their area

## Best Practices

### For Emergency Responders

1. **Act Quickly** - Dispatch ambulances immediately for critical cases
2. **Update Status** - Keep status current so others know what's happening
3. **Document Details** - Add description and location information
4. **Follow Up** - Create patient records for anonymous callers

### For Administrators

1. **Monitor Active Calls** - Check emergency list regularly
2. **Review Response Times** - Identify bottlenecks
3. **Train Staff** - Ensure all authorized users know the workflow
4. **Audit Records** - Review completed emergencies for quality assurance

---

**Last Updated:** November 23, 2025

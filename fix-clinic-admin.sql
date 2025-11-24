-- Fix Clinic Admin Assignment
-- Run this SQL to assign Sbusiso Madikizela to Soweto Community Clinic

-- First, let's see the clinic ID
SELECT Id, Name FROM Clinics;

-- Then update the user (replace the GUIDs with actual values from above)
-- UPDATE Users 
-- SET ClinicId = '<clinic-guid-from-above>' 
-- WHERE Email = 'Madikizela21517799@gmail.com';

-- Verify the update
SELECT Email, FirstName, LastName, Role, ClinicId FROM Users;

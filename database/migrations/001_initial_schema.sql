-- Manual migration script (reference only - use EF Core migrations)
-- This shows the expected database structure

CREATE TABLE patients (
    id UUID PRIMARY KEY,
    id_number VARCHAR(20) UNIQUE NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    date_of_birth DATE NOT NULL,
    phone_number VARCHAR(20) NOT NULL,
    alternative_phone_number VARCHAR(20),
    preferred_language VARCHAR(5) DEFAULT 'en',
    home_address TEXT,
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    chronic_conditions JSONB DEFAULT '[]',
    allergies JSONB DEFAULT '[]',
    blood_type VARCHAR(5),
    emergency_contact_name VARCHAR(100),
    emergency_contact_phone VARCHAR(20),
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP
);

CREATE TABLE medications (
    id UUID PRIMARY KEY,
    patient_id UUID NOT NULL REFERENCES patients(id),
    name VARCHAR(200) NOT NULL,
    dosage VARCHAR(100) NOT NULL,
    frequency VARCHAR(50) NOT NULL,
    reminder_times JSONB NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE,
    is_active BOOLEAN DEFAULT true,
    instructions TEXT,
    created_at TIMESTAMP NOT NULL
);

CREATE TABLE medication_logs (
    id UUID PRIMARY KEY,
    medication_id UUID NOT NULL REFERENCES medications(id),
    scheduled_time TIMESTAMP NOT NULL,
    taken_time TIMESTAMP,
    was_taken BOOLEAN DEFAULT false,
    notes TEXT,
    recorded_by_user_id UUID
);

CREATE TABLE clinics (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    phone_number VARCHAR(20) NOT NULL,
    address TEXT NOT NULL,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,
    operating_hours TEXT,
    has_ambulance BOOLEAN DEFAULT false,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP NOT NULL
);

CREATE TABLE appointments (
    id UUID PRIMARY KEY,
    patient_id UUID NOT NULL REFERENCES patients(id),
    clinic_id UUID REFERENCES clinics(id),
    scheduled_date_time TIMESTAMP NOT NULL,
    appointment_type VARCHAR(50) NOT NULL,
    status VARCHAR(20) DEFAULT 'scheduled',
    notes TEXT,
    completed_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL
);

CREATE TABLE emergency_calls (
    id UUID PRIMARY KEY,
    patient_id UUID NOT NULL REFERENCES patients(id),
    emergency_type VARCHAR(20) DEFAULT 'medical',
    call_time TIMESTAMP NOT NULL,
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    status VARCHAR(20) DEFAULT 'pending',
    description TEXT,
    assigned_ambulance_id UUID,
    dispatched_at TIMESTAMP,
    arrived_at TIMESTAMP,
    completed_at TIMESTAMP
);

CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    role VARCHAR(50) DEFAULT 'patient',
    phone_number VARCHAR(20),
    clinic_id UUID REFERENCES clinics(id),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP NOT NULL,
    last_login_at TIMESTAMP
);

-- Indexes
CREATE INDEX idx_patients_phone ON patients(phone_number);
CREATE INDEX idx_patients_id_number ON patients(id_number);
CREATE INDEX idx_medications_patient ON medications(patient_id);
CREATE INDEX idx_medication_logs_medication ON medication_logs(medication_id, scheduled_time);
CREATE INDEX idx_appointments_scheduled ON appointments(scheduled_date_time);
CREATE INDEX idx_emergency_calls_time ON emergency_calls(call_time);
CREATE INDEX idx_emergency_calls_status ON emergency_calls(status);

-- Initial database setup script for PostgreSQL
-- Run this after creating the database

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable PostGIS for location features (optional, for future use)
-- CREATE EXTENSION IF NOT EXISTS postgis;

-- Create enum types
CREATE TYPE user_role AS ENUM ('patient', 'caregiver', 'health_worker', 'clinic_admin', 'system_admin');
CREATE TYPE emergency_type AS ENUM ('medical', 'security');
CREATE TYPE call_status AS ENUM ('pending', 'dispatched', 'arrived', 'completed', 'cancelled');
CREATE TYPE appointment_status AS ENUM ('scheduled', 'completed', 'missed', 'cancelled');

-- Indexes will be created by EF Core migrations
-- This file is for reference and manual setup if needed

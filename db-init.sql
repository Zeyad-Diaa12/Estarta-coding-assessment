-- Create Users table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(100) NOT NULL,
    "NationalNumber" VARCHAR(50) UNIQUE NOT NULL,
    "Email" VARCHAR(100) NOT NULL,
    "Phone" VARCHAR(20) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true
);

-- Create Salaries table
CREATE TABLE IF NOT EXISTS "Salaries" (
    "Id" SERIAL PRIMARY KEY,
    "Year" INTEGER NOT NULL,
    "Month" INTEGER NOT NULL CHECK ("Month" BETWEEN 1 AND 12),
    "Salary" NUMERIC(18, 2) NOT NULL,
    "UserId" INTEGER NOT NULL,
    CONSTRAINT "FK_Salaries_Users" FOREIGN KEY ("UserId") 
        REFERENCES "Users"("Id") ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_Users_NationalNumber" ON "Users"("NationalNumber");
CREATE INDEX IF NOT EXISTS "IX_Salaries_UserId" ON "Salaries"("UserId");
CREATE INDEX IF NOT EXISTS "IX_Salaries_Year_Month" ON "Salaries"("Year", "Month");

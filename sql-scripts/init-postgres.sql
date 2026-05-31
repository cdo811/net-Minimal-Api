CREATE TABLE public.Customers
(
    CustomerID     SERIAL PRIMARY KEY,
    Gender         VARCHAR(10),
    Age            INT,
    AnnualIncome   INT,
    SpendingScore  INT,
    Profession     VARCHAR(50),
    WorkExperience INT,
    FamilySize     INT
);

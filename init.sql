create table dbo.Customers
(
    CustomerID     int not null identity
        primary key,
    Gender         varchar(10),
    Age            int,
    AnnualIncome   int,
    SpendingScore  int,
    Profession     varchar(50),
    WorkExperience int,
    FamilySize     int
)
go


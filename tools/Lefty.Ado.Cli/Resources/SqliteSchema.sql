--
-- ADO Schema
-- Mapped 1:1 with the model classes from Lefty.Ado.Abstractions
--

create table if not exists Iterations
(
    Id text not null primary key,
    Name text not null,
    DateStart date null,
    DateEnd date null
);

create table if not exists AppUsers
(
    Id text not null primary key,
    DisplayName text not null,
    Upn text not null
);

create table if not exists WorkItems
(
    Id integer not null primary key,
    Title text not null,
    Description text not null,
    State text not null,
    CreatedByUserId text not null references AppUsers (Id),
    MomentCreated datetime not null,
    MomentActivity datetime not null,
    AssignedToUserId text null references AppUsers (Id),
    Tags text not null,
    IterationId text null references Iterations (Id),
    IssueType text null,
    Component text null,
    Severity text null
);

create table if not exists WorkItemRemarks
(
    ItemId integer not null references WorkItems (Id),
    Text text not null,
    ByUserId text not null references AppUsers (Id),
    Moment datetime not null
);

create table if not exists WorkItemTransitions
(
    ItemId integer not null references WorkItems (Id),
    [From] text not null,
    [To] text not null,
    ByUserId text not null references AppUsers (Id),
    Moment datetime not null
);

-- eof

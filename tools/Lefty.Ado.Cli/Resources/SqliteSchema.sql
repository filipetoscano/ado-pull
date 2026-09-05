--
-- ADO Schema
-- Mapped 1:1 with the model classes from Lefty.Ado.Abstractions
--

create table if not exists Iteration
(
    Id text not null primary key,
    Name text not null,
    DateStart date null,
    DateEnd date null
);

create table if not exists AppUser
(
    Id text not null primary key,
    DisplayName text not null,
    Upn text not null
);

create table if not exists WorkItem
(
    Id integer not null primary key,
    Title text not null,
    Description text not null,
    State text not null,
    CreatedByUserId text not null references AppUser (Id),
    MomentCreated datetime not null,
    MomentActivity datetime not null,
    AssignedToUserId text null references AppUser (Id),
    Tags text not null,
    IterationId text null references Iteration (Id),
    IssueType text null,
    Component text null,
    Severity text null
);

create table if not exists WorkItemRemark
(
    ItemId integer not null references WorkItem (Id),
    Text text not null,
    ByUserId text not null references AppUser (Id),
    Moment datetime not null
);

create table if not exists WorkItemTransition
(
    ItemId integer not null references WorkItem (Id),
    [From] text not null,
    [To] text not null,
    ByUserId text not null references AppUser (Id),
    Moment datetime not null
);

-- eof
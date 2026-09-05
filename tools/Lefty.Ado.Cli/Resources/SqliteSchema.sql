--
-- ADO Schema
-- Mapped 1:1 with the model classes from Lefty.Ado.Abstractions
--

create table Iteration
(
    Name text not null,
    DateStart date null,
    DateEnd date null
);

create table AppUser
(
    DisplayName text not null,
    Upn text not null,
);

create table WorkItem
(
    Id number not null,
);

create table WorkItemRemark
(
    ItemId number not null,
);

create table WorkItemTransition
(
    ItemId number not null,
);

-- eof
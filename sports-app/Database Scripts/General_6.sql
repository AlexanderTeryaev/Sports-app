  alter table Unions
  add UnionNumber bigint null
  GO

  alter table Games drop column Rounds
  alter table Games drop column PlayoffTeamsNum
  alter table Games drop column WeekRounds
  alter table Games drop column NumberOfSequenceRounds
  alter table Games drop column GroupsNum
  GO

  alter table Games add ActiveWeeksNumber int not null default 0
  alter table Games add BreakWeeksNumber int not null default 0
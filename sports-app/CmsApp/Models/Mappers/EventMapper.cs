using AppModel;
using System;

namespace CmsApp.Models.Mappers
{
    public static class EventMapper
    {
        public static DateTime RoundUp(this DateTime dt, TimeSpan d)
        {
            return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
        }

        public static Event ToEvent(this EventForm ef)
        {
            return new Event {
                CreateDate = ef.CreateDate == default(DateTime) ?
                    DateTime.Now : ef.CreateDate,
                EventId = ef.EventId,
                EventTime = ef.EventTime,
                IsPublished = ef.IsPublished,
                LeagueId = ef.LeagueId,
                ClubId = ef.ClubId,
                Place = ef.Place,
                Title = ef.Title,
                EventDescription = ef.Description,
                UnionId = ef.UnionId
            };
        }

        public static EventForm ToEventForm(this Event ev)
        {
            return new EventForm
            {
                CreateDate = ev.CreateDate,
                EventId = ev.EventId,
                EventTime = ev.EventTime,
                IsPublished = ev.IsPublished,
                LeagueId = ev.LeagueId,
                ClubId = ev.ClubId,
                LeagueName = ev?.League?.Name,
                ClubName = ev?.Club?.Name,
                Place = ev.Place,
                Title = ev.Title
            };
        } 
    }
}
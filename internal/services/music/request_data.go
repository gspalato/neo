package music

import (
	"time"
)

type TrackRequestData struct {
	RequestedAt time.Time `json:"requested_at"`
	AuthorID    string    `json:"author_id"`
}

func NewTrackRequestData(authorID string, timestamp *time.Time) TrackRequestData {
	var t time.Time
	if timestamp == nil {
		t = time.Now()
	} else {
		t = *timestamp
	}

	return TrackRequestData{
		RequestedAt: t,
		AuthorID:    authorID,
	}
}

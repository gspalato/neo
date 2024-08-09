package music

import (
	"time"

	"github.com/disgoorg/disgolink/v3/lavalink"
)

type MusicRequest struct {
	Track *lavalink.Track

	CreatedAt time.Time
	AuthorID  string
}

func NewMusicRequest(track *lavalink.Track, authorID string) *MusicRequest {
	return &MusicRequest{
		Track:     track,
		CreatedAt: time.Now(),
		AuthorID:  authorID,
	}
}

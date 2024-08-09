package datetime

import (
	"fmt"
	"time"

	"github.com/disgoorg/disgolink/v3/lavalink"
)

func ToDuration(d lavalink.Duration) time.Duration {
	return time.Duration(d.Milliseconds() * 1000000)
}

// A function that prints durations with the format "3m 2s", omitting 0 values.
func Pretty(dur time.Duration) string {
	var output string

	d := dur.Hours() / 24
	h := dur.Hours()
	m := dur.Minutes()
	s := dur.Seconds()
	ms := dur.Milliseconds()

	if d >= 1 {
		output += fmt.Sprintf("%dd ", int(d))
	}

	if h >= 1 {
		output += fmt.Sprintf("%dh ", int(h))
	}

	if m >= 1 {
		output += fmt.Sprintf("%dm ", int(m)%60)
	}

	if s >= 1 {
		output += fmt.Sprintf("%ds", int(s)%60)
	} else if ms >= 1 {
		output += fmt.Sprintf("%dms", int(ms)%1000)
	}

	return output
}

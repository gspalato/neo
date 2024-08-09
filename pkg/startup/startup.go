package startup

import "time"

var startTime = time.Now()

func Took() time.Duration {
	return time.Since(startTime)
}

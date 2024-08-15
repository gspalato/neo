package colorutils

import (
	"image"
	"image/color"
	"net/http"
	"sort"
	"strconv"

	"github.com/cenkalti/dominantcolor"
	"github.com/lucasb-eyer/go-colorful"

	sliceutils "unreal.sh/neo/internal/utils/sliceutils"
)

func GetDominantColorFromImageURL(url string) (int, error) {
	response, e := http.Get(url)
	if e != nil {
		return 0, e
	}
	defer response.Body.Close()

	img, _, err := image.Decode(response.Body)
	if err != nil {
		return 0, err
	}

	colors := sliceutils.Map(dominantcolor.FindN(img, 5), func(color color.RGBA) colorful.Color {
		hex := dominantcolor.Hex(color)
		c, _ := colorful.Hex(hex)
		return c
	})

	// Sort colors by saturation and value
	sort.Slice(colors, func(i, j int) bool {
		_, cis, civ := colors[i].Hsv()
		_, cjs, cjv := colors[j].Hsv()

		return cis*civ > cjs*cjv
	})

	c, err := strconv.ParseInt(colors[0].Hex()[1:], 16, 64)

	if err != nil {
		return 0, err
	}

	color := int(c)

	return color, nil
}

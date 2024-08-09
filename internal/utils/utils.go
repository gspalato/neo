package utils

func MUST(err error) {
	if err != nil {
		panic(err)
	}
}

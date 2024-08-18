package slice

// Maps a function to all elements of a slice and returns a new slice with the results.
func Map[T, U any](s []T, f func(T) U) []U {
	m := make([]U, len(s))
	for i := range s {
		m[i] = f(s[i])
	}

	return m
}

// Filters a slice based on a predicate function.
func Filter[T any](s []T, f func(T) bool) []T {
	m := make([]T, 0)
	for _, v := range s {
		if f(v) {
			m = append(m, v)
		}
	}

	return m
}

// Returns an array of slices of size chunkSize.
func Chunk[T any](items []T, chunkSize int) (chunks [][]T) {
	for chunkSize < len(items) {
		items, chunks = items[chunkSize:], append(chunks, items[0:chunkSize:chunkSize])
	}
	return append(chunks, items)
}

func Any[T comparable](s []T, f func(T) bool) bool {
	for _, v := range s {
		if f(v) {
			return true
		}
	}

	return false
}

func Contains[T comparable](s []T, e T) bool {
	return Any(s, func(a T) bool {
		return a == e
	})
}

// Checks if the subset is a subset of the superset.
// This effectively means that it checks if all elements of the subset are present in the superset.
func IsSubset[T comparable](subset []T, superset []T) bool {
	checkset := make(map[T]bool)
	for _, element := range subset {
		checkset[element] = true
	}

	for _, value := range superset {
		if checkset[value] {
			delete(checkset, value)
		}
	}

	return len(checkset) == 0 // this implies that set is subset of superset
}

// Checks if any element from a subset is present in the superset.
func IsAnySubset[T comparable](subset []T, superset []T) bool {
	// Create a map to store elements of superset
	elementMap := make(map[T]bool)

	// Populate the map with elements of superset
	for _, b := range superset {
		elementMap[b] = true
	}

	// Check if any element in subset exists in the map
	for _, a := range subset {
		if _, exists := elementMap[a]; exists {
			return true
		}
	}

	return false
}

func Difference[T comparable](slice1 []T, slice2 []T) []T {
	var diff []T

	// Loop two times, first to find slice1 strings not in slice2,
	// second loop to find slice2 strings not in slice1
	for i := 0; i < 2; i++ {
		for _, s1 := range slice1 {
			found := false
			for _, s2 := range slice2 {
				if s1 == s2 {
					found = true
					break
				}
			}
			// String not found. We add it to return slice
			if !found {
				diff = append(diff, s1)
			}
		}
		// Swap the slices, only if it was the first loop
		if i == 0 {
			slice1, slice2 = slice2, slice1
		}
	}

	return diff
}

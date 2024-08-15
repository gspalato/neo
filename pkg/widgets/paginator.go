package widgets

import (
	"fmt"
	"log/slog"
	"sync"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
)

// Paginator provides a method for creating a navigatable embed
type Paginator struct {
	sync.Mutex
	Pages []*discordgo.MessageEmbed
	Index int

	// Loop back to the beginning or end when on the first or last page.
	Loop   bool
	Widget *Widget

	Session *discordgo.Session

	DeleteMessageWhenDone bool
	DeleteButtonsWhenDone bool

	running bool
}

// NewPaginator returns a new Paginator
//
//	ses      : discordgo session
//	channelID: channelID to spawn the paginator on
func NewPaginator(ctx *ken.Context) *Paginator {
	session := (*ctx).GetSession()

	p := &Paginator{
		Session:               session,
		Pages:                 []*discordgo.MessageEmbed{},
		Index:                 0,
		Loop:                  false,
		DeleteMessageWhenDone: false,
		DeleteButtonsWhenDone: false,
		Widget:                NewWidget(session, ctx, nil),
	}

	p.addHandlers()

	p.Widget.WhitelistUser((*ctx).User().ID)

	return p
}

func (p *Paginator) addHandlers() {
	p.Widget.AddOption(func(w *Widget) (discordgo.MessageComponent, ken.ComponentHandlerFunc) {
		button := &discordgo.Button{
			Style:    discordgo.SecondaryButton,
			Label:    NavBeginning,
			CustomID: "nav_beginning",
		}

		return button, func(ctx ken.ComponentContext) bool {
			if err := p.Goto(0); err == nil {
				p.Update()
				return true
			} else {
				return false
			}
		}
	})

	p.Widget.AddOption(func(w *Widget) (discordgo.MessageComponent, ken.ComponentHandlerFunc) {
		button := &discordgo.Button{
			Style:    discordgo.SecondaryButton,
			Label:    NavLeft,
			CustomID: "nav_left",
		}

		return button, func(ctx ken.ComponentContext) bool {
			if err := p.PreviousPage(); err == nil {
				p.Update()
				return true
			} else {
				return false
			}
		}
	})

	p.Widget.AddOption(func(w *Widget) (discordgo.MessageComponent, ken.ComponentHandlerFunc) {
		button := &discordgo.Button{
			Style:    discordgo.SecondaryButton,
			Label:    NavRight,
			CustomID: "nav_right",
		}

		return button, func(ctx ken.ComponentContext) bool {
			if err := p.NextPage(); err == nil {
				p.Update()
				return true
			} else {
				return false
			}
		}
	})

	p.Widget.AddOption(func(w *Widget) (discordgo.MessageComponent, ken.ComponentHandlerFunc) {
		button := &discordgo.Button{
			Style:    discordgo.SecondaryButton,
			Label:    NavEnd,
			CustomID: "nav_end",
		}

		return button, func(ctx ken.ComponentContext) bool {
			if err := p.Goto(len(p.Pages) - 1); err == nil {
				p.Update()
				return true
			} else {
				return false
			}
		}
	})
}

// Spawn spawns the paginator in channel p.ChannelID
func (p *Paginator) Spawn() error {
	if p.Running() {
		return ErrAlreadyRunning
	}

	p.Lock()
	p.running = true
	p.Unlock()

	defer func() {
		p.Lock()
		p.running = false
		p.Unlock()

		// Delete message when done
		if p.DeleteMessageWhenDone && p.Widget.Message != nil {
			p.Session.ChannelMessageDelete(p.Widget.Message.ChannelID, p.Widget.Message.ID)
		}

		// Delete buttons when done
		if p.DeleteButtonsWhenDone && p.Widget.Message != nil {
			embed, err := p.Page()
			if err != nil {
				return
			}

			p.Session.ChannelMessageEditComplex(&discordgo.MessageEdit{
				Embeds:     &[]*discordgo.MessageEmbed{embed},
				Components: &[]discordgo.MessageComponent{},
			})
		}
	}()

	page, err := p.Page()
	if err != nil {
		slog.Error("Failed to get current page.")
		return err
	}
	p.Widget.Embed = page

	return p.Widget.Spawn()
}

// Add a page to the paginator
//
//	embed: embed page to add.
func (p *Paginator) Add(embeds ...*discordgo.MessageEmbed) {
	p.Pages = append(p.Pages, embeds...)
}

// Page returns the page of the current index
func (p *Paginator) Page() (*discordgo.MessageEmbed, error) {
	p.Lock()
	defer p.Unlock()

	if p.Index < 0 || p.Index >= len(p.Pages) {
		return nil, ErrIndexOutOfBounds
	}

	return p.Pages[p.Index], nil
}

// NextPage sets the page index to the next page
func (p *Paginator) NextPage() error {
	p.Lock()
	defer p.Unlock()

	if p.Index+1 >= 0 && p.Index+1 < len(p.Pages) {
		p.Index++
		return nil
	}

	// Set the queue back to the beginning if Loop is enabled.
	if p.Loop {
		p.Index = 0
		return nil
	}

	return ErrIndexOutOfBounds
}

// PreviousPage sets the current page index to the previous page.
func (p *Paginator) PreviousPage() error {
	p.Lock()
	defer p.Unlock()

	if p.Index-1 >= 0 && p.Index-1 < len(p.Pages) {
		p.Index--
		return nil
	}

	// Set the queue back to the beginning if Loop is enabled.
	if p.Loop {
		p.Index = len(p.Pages) - 1
		return nil
	}

	return ErrIndexOutOfBounds
}

// Goto jumps to the requested page index
//
//	index: The index of the page to go to
func (p *Paginator) Goto(index int) error {
	p.Lock()
	defer p.Unlock()
	if index < 0 || index >= len(p.Pages) {
		return ErrIndexOutOfBounds
	}
	p.Index = index
	return nil
}

// Update updates the message with the current state of the paginator
func (p *Paginator) Update() error {
	if p.Widget.Message == nil {
		return ErrNilMessage
	}

	page, err := p.Page()
	if err != nil {
		return err
	}

	_, err = p.Widget.UpdateEmbed(page)
	return err
}

// Running returns the running status of the paginator
func (p *Paginator) Running() bool {
	p.Lock()
	running := p.running
	p.Unlock()
	return running
}

// SetPageFooters sets the footer of each embed to
// Be its page number out of the total length of the embeds.
func (p *Paginator) SetPageFooters() {
	for index, embed := range p.Pages {
		embed.Footer = &discordgo.MessageEmbedFooter{
			Text: fmt.Sprintf("#[%d / %d]", index+1, len(p.Pages)),
		}
	}
}

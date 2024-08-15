package widgets

import (
	"errors"
	"log/slog"
	"sync"
	"time"

	"github.com/bwmarrin/discordgo"
	"github.com/zekrotja/ken"
)

// error vars
var (
	ErrAlreadyRunning   = errors.New("err: Widget already running")
	ErrIndexOutOfBounds = errors.New("err: Index is out of bounds")
	ErrNilMessage       = errors.New("err: Message is nil")
	ErrNilEmbed         = errors.New("err: embed is nil")
	ErrNotRunning       = errors.New("err: not running")
)

const (
	NavPlus        = "➕"
	NavPlay        = "▶"
	NavPause       = "⏸"
	NavStop        = "⏹"
	NavRight       = "➡"
	NavLeft        = "⬅"
	NavUp          = "⬆"
	NavDown        = "⬇"
	NavEnd         = "⏩"
	NavBeginning   = "⏪"
	NavNumbers     = "🔢"
	NavInformation = "ℹ"
	NavSave        = "💾"
)

// WidgetOption is a function that returns a discordgo.MessageComponent and a ken.ComponentHandlerFunc.
type WidgetOption func(*Widget) (discordgo.MessageComponent, ken.ComponentHandlerFunc)

// Widget is a message embed with reactions for buttons.
// Accepts custom handlers for reactions.
type Widget struct {
	sync.Mutex
	Embed   *discordgo.MessageEmbed
	Message *discordgo.Message
	Session *discordgo.Session
	Context *ken.Context
	Timeout time.Duration
	Close   chan bool

	Options []WidgetOption

	// Keys stores the handlers keys in the order they were added
	Keys []string

	// Determines if the widget can be interacted with only once.
	Once bool
	// Delete reactions after they are added
	DeleteReactions bool
	// Only allow listed users to use reactions.
	UserWhitelist []string

	running bool
}

// NewWidget returns a pointer to a Widget object
//
//	ses      : discordgo session
//	channelID: channelID to spawn the widget on
func NewWidget(session *discordgo.Session, ctx *ken.Context, embed *discordgo.MessageEmbed) *Widget {
	return &Widget{
		Context:         ctx,
		Session:         session,
		Keys:            []string{},
		Options:         make([]WidgetOption, 0),
		Close:           make(chan bool),
		DeleteReactions: true,
		Embed:           embed,
		Once:            false,
	}
}

// isUserAllowed returns true if the user is allowed
// to use this widget.
func (w *Widget) isUserAllowed(userID string) bool {
	if w.UserWhitelist == nil || len(w.UserWhitelist) == 0 {
		return true
	}
	for _, user := range w.UserWhitelist {
		if user == userID {
			return true
		}
	}
	return false
}

func (w *Widget) WhitelistUser(userID string) {
	w.UserWhitelist = append(w.UserWhitelist, userID)
}

func (w *Widget) Spawn() error {
	if w.Running() {
		return ErrAlreadyRunning
	}

	w.running = true
	defer func() {
		w.running = false
	}()

	if w.Embed == nil {
		return ErrNilEmbed
	}

	b := (*w.Context).FollowUpEmbed(w.Embed)
	/*
		b.AddComponents(func(c *ken.ComponentBuilder) {
			for _, opt := range w.Options {
				component, handler := opt(w)
				c.Add(component, handler, w.Once).Condition(func(cctx ken.ComponentContext) bool {
					return w.isUserAllowed(cctx.User().ID)
				})
			}
		})
	*/
	msg := b.Send()

	if msg.Error != nil {
		slog.Error("Widget failed to send message")
		slog.Error(msg.Error.Error())
	}

	return msg.Error
}

func (w *Widget) AddOption(opt WidgetOption) {
	w.Options = append(w.Options, opt)
}

// Running returns w.running
func (w *Widget) Running() bool {
	w.Lock()
	running := w.running
	w.Unlock()
	return running
}

// UpdateEmbed updates the embed object and edits the original message
//
//	embed: New embed object to replace w.Embed
func (w *Widget) UpdateEmbed(embed *discordgo.MessageEmbed) (*discordgo.Message, error) {
	if w.Message == nil {
		return nil, ErrNilMessage
	}
	return w.Session.ChannelMessageEditEmbed((*w.Context).GetEvent().ChannelID, w.Message.ID, embed)
}

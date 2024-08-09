package services

import "github.com/zekrotja/ken"

var (
	_ ken.ObjectProvider = (*ServiceProvider)(nil)
)

type ServiceProvider struct {
	services map[string]interface{}
}

func NewServiceProvider() *ServiceProvider {
	return &ServiceProvider{
		services: make(map[string]interface{}),
	}
}

func (p *ServiceProvider) Get(key string) interface{} {
	return p.services[key]
}

func (p *ServiceProvider) Register(key string, service interface{}) {
	p.services[key] = service
}

func (p *ServiceProvider) Unregister(key string) {
	delete(p.services, key)
}

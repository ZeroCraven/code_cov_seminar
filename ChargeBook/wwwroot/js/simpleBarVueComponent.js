﻿Vue.component('simplebar', {
    name: "simpleBar",
    data: function () {
        return {};
    },
    computed: {
        scrollElement () {
            return this.$refs.scrollElement;
        },
        contentElement () {
            return this.$refs.contentElement;
        }
    },
    mounted() {
        const options = SimpleBar.getOptions(this.$refs.element.attributes);
        this.SimpleBar = new SimpleBar(this.$refs.element, options);
    },
    template: `
        <div ref="element">
            <div class="simplebar-wrapper">
                <div class="simplebar-height-auto-observer-wrapper">
                    <div class="simplebar-height-auto-observer" />
                </div>
                <div class="simplebar-mask">
                    <div class="simplebar-offset">
                        <div class="simplebar-content-wrapper" ref="scrollElement">
                            <div class="simplebar-content" ref="contentElement">
                                <slot></slot>
                            </div>
                        </div>
                  </div>
                </div>
                <div class="simplebar-placeholder" />
                </div>
                <div class="simplebar-track simplebar-horizontal">
                <div class="simplebar-scrollbar" />
                </div>
                <div class="simplebar-track simplebar-vertical">
                <div class="simplebar-scrollbar" />
            </div>
        </div>
    `
});
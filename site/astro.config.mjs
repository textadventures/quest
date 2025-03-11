// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
	integrations: [
		starlight({
			title: 'Quest Viva',
			social: {
				github: 'https://github.com/textadventures/quest',
			},
			sidebar: [
				{
					label: 'Guides',
					autogenerate: { directory: 'guides' },
				},
				{
					label: 'Project',
					autogenerate: { directory: 'project' },
				},
			],
		}),
	],
});

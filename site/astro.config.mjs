// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
  integrations: [
    starlight({
      title: 'Quest Viva',
      logo: {
        src: './src/assets/quest-viva.svg',
      },
      social: {
        github: 'https://github.com/textadventures/quest',
        discord: 'https://textadventures.co.uk/community/discord',
      },
      editLink: {
        baseUrl: 'https://github.com/textadventures/quest/edit/main/site/',
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

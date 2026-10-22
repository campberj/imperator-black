// The real compiled content, typed and indexed, for component tests: they fail if the views and content diverge.
import json from '../../../content/dist/content.json'
import { indexContent, type Content } from './content'

export const content = json as unknown as Content
export const lookup = indexContent(content)

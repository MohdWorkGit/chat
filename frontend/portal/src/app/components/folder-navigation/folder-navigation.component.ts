import {
  Component,
  ChangeDetectionStrategy,
  OnInit,
  Input,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { PortalApiService, Category } from '../../services/portal-api.service';

export interface FolderNode {
  id: number;
  name: string;
  slug: string;
  expanded: boolean;
}

@Component({
  selector: 'portal-folder-navigation',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <nav class="folder-nav" aria-label="Category navigation">
      <div class="folder-nav-title">Categories</div>
      <ul class="folder-nav-list">
        @for (folder of folders(); track folder.id) {
          <li class="folder-nav-item">
            <a
              class="folder-nav-link"
              [routerLink]="['/category', folder.slug]"
              routerLinkActive="folder-nav-link-active"
              (click)="toggleFolder(folder)">
              <span class="folder-nav-icon" [class.expanded]="folder.expanded">
                <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z"/>
                </svg>
              </span>
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="currentColor" class="folder-nav-folder-icon">
                <path d="M10 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2h-8l-2-2z"/>
              </svg>
              <span class="folder-nav-name">{{ folder.name }}</span>
            </a>
          </li>
        }
      </ul>
    </nav>
  `,
  styles: [`
    .folder-nav {
      padding: 16px 0;
    }
    .folder-nav-title {
      font-weight: 600;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--portal-text-secondary, #6b7280);
      padding: 0 16px 12px;
    }
    .folder-nav-list {
      list-style: none;
      padding: 0;
      margin: 0;
    }
    .folder-nav-item {
      margin: 0;
    }
    .folder-nav-link {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 16px;
      font-size: 0.875rem;
      color: var(--portal-text, #111827);
      text-decoration: none;
      border-radius: 0;
      transition: background-color 0.15s, color 0.15s;
      cursor: pointer;
    }
    .folder-nav-link:hover {
      background-color: var(--portal-bg, #f9fafb);
      text-decoration: none;
      color: var(--portal-primary, #1b72e8);
    }
    .folder-nav-link-active {
      background-color: rgba(27, 114, 232, 0.06);
      color: var(--portal-primary, #1b72e8);
      font-weight: 500;
    }
    .folder-nav-link-active .folder-nav-folder-icon {
      color: var(--portal-primary, #1b72e8);
    }
    .folder-nav-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      color: var(--portal-text-secondary, #6b7280);
      transition: transform 0.2s ease;
    }
    .folder-nav-icon.expanded {
      transform: rotate(90deg);
    }
    .folder-nav-folder-icon {
      flex-shrink: 0;
      color: var(--portal-text-secondary, #6b7280);
    }
    .folder-nav-name {
      flex: 1;
      min-width: 0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FolderNavigationComponent implements OnInit {
  @Input() activeCategorySlug = '';

  folders = signal<FolderNode[]>([]);

  constructor(private readonly apiService: PortalApiService) {}

  ngOnInit(): void {
    this.apiService.getCategories().subscribe(categories => {
      const folderNodes: FolderNode[] = categories.map(cat => ({
        id: cat.id,
        name: cat.name,
        slug: cat.slug,
        expanded: cat.slug === this.activeCategorySlug,
      }));
      this.folders.set(folderNodes);
    });
  }

  toggleFolder(folder: FolderNode): void {
    this.folders.update(folders =>
      folders.map(f =>
        f.id === folder.id ? { ...f, expanded: !f.expanded } : f,
      ),
    );
  }
}

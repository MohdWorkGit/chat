import {
  Component,
  ChangeDetectionStrategy,
  OnInit,
  Input,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { PortalApiService, Category } from '../../services/portal-api.service';

export interface FolderNode {
  id: number;
  name: string;
  slug: string;
  articleCount: number;
  parentCategoryId: number | null;
  expanded: boolean;
  children: FolderNode[];
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
          <ng-container *ngTemplateOutlet="folderItem; context: { $implicit: folder }"></ng-container>
        }
      </ul>
    </nav>

    <ng-template #folderItem let-folder>
      <li class="folder-nav-item">
        <a
          class="folder-nav-link"
          [routerLink]="['/category', folder.slug]"
          routerLinkActive="folder-nav-link-active"
          (click)="toggleFolder(folder)">
          @if (folder.children.length > 0) {
            <span class="folder-nav-icon" [class.expanded]="folder.expanded">
              <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="currentColor">
                <path d="M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z"/>
              </svg>
            </span>
          } @else {
            <span class="folder-nav-icon folder-nav-icon-spacer"></span>
          }
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="currentColor" class="folder-nav-folder-icon">
            <path d="M10 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2h-8l-2-2z"/>
          </svg>
          <span class="folder-nav-name">{{ folder.name }}</span>
          <span class="folder-nav-count">{{ folder.articleCount }}</span>
        </a>
        @if (folder.expanded && folder.children.length > 0) {
          <ul class="folder-nav-sublist">
            @for (child of folder.children; track child.id) {
              <ng-container *ngTemplateOutlet="folderItem; context: { $implicit: child }"></ng-container>
            }
          </ul>
        }
      </li>
    </ng-template>
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
    .folder-nav-icon-spacer {
      width: 12px;
      height: 12px;
    }
    .folder-nav-sublist {
      list-style: none;
      padding: 0 0 0 20px;
      margin: 0;
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
    .folder-nav-count {
      flex-shrink: 0;
      font-size: 0.75rem;
      color: var(--portal-text-secondary, #6b7280);
      background-color: var(--portal-bg, #f9fafb);
      padding: 2px 8px;
      border-radius: 10px;
      min-width: 24px;
      text-align: center;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FolderNavigationComponent implements OnInit {
  @Input() activeCategorySlug = '';

  folders = signal<FolderNode[]>([]);

  constructor(private readonly apiService: PortalApiService) {}

  ngOnInit(): void {
    this.apiService.getCategories()
      .pipe(catchError(() => of<Category[]>([])))
      .subscribe(categories => {
        this.folders.set(this.buildTree(categories));
      });
  }

  private buildTree(categories: Category[]): FolderNode[] {
    const nodesById = new Map<number, FolderNode>();
    for (const cat of categories) {
      nodesById.set(cat.id, {
        id: cat.id,
        name: cat.name,
        slug: cat.slug,
        articleCount: cat.articleCount,
        parentCategoryId: cat.parentCategoryId ?? null,
        expanded: false,
        children: [],
      });
    }

    const roots: FolderNode[] = [];
    for (const node of nodesById.values()) {
      const parent = node.parentCategoryId != null ? nodesById.get(node.parentCategoryId) : undefined;
      if (parent) {
        parent.children.push(node);
      } else {
        roots.push(node);
      }
    }

    if (this.activeCategorySlug) {
      this.expandAncestors(roots, this.activeCategorySlug);
    }

    return roots;
  }

  private expandAncestors(nodes: FolderNode[], activeSlug: string): boolean {
    for (const node of nodes) {
      if (node.slug === activeSlug) {
        node.expanded = true;
        return true;
      }
      if (node.children.length > 0 && this.expandAncestors(node.children, activeSlug)) {
        node.expanded = true;
        return true;
      }
    }
    return false;
  }

  toggleFolder(folder: FolderNode): void {
    this.folders.update(folders => this.toggleInTree(folders, folder.id));
  }

  private toggleInTree(nodes: FolderNode[], targetId: number): FolderNode[] {
    return nodes.map(node => {
      if (node.id === targetId) {
        return { ...node, expanded: !node.expanded };
      }
      if (node.children.length > 0) {
        return { ...node, children: this.toggleInTree(node.children, targetId) };
      }
      return node;
    });
  }
}

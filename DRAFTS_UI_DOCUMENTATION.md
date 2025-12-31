# Drafts Page UI Documentation

## Overview
The Drafts page is an admin-only interface for managing draft content before publication.

## Page Layout

### Header Section
- **Page Title**: "Draft Content Management" (h1)
- **Admin Notice**: Yellow banner with shield icon indicating "Admin Section - Only visible to administrators"

### Draft Cards Grid
The drafts are displayed in a responsive grid layout (CSS Grid with auto-fill, min 400px columns).

## Draft Card Components

Each draft is shown in a Material Design card with the following sections:

### Card Header
- **Title**: Large, bold text (e.g., "Getting Started with Angular")
- **Subtitle**: Smaller, grey text below title (e.g., "A comprehensive guide for beginners")

### Card Content

#### Author Information
- Icon: Person icon
- Text: Author name (e.g., "John Doe")

#### Date Information  
- Icon: Calendar icon
- Text: "Created: Dec 29, 2025, 10:00 AM" (formatted with Angular date pipe)

#### Scheduled Publication (if set)
- Icon: Schedule/clock icon
- Text: "Scheduled: Jan 2, 2026, 3:00 PM"
- Only displayed if PublishAt is set

#### Content Preview
- Grey background box
- First ~200 characters of the content
- Basic markdown formatting removed
- Ends with "..." if truncated

#### Draft Badge
- Material chip with "DRAFT" text
- Accent color (typically orange/yellow)
- Highlighted style

### Card Actions (Footer)

Four action buttons arranged horizontally:

1. **Publish Now** (Primary button - Blue)
   - Icon: Publish icon
   - Action: Sets draft=false, active=true
   - Makes content immediately visible on public blog

2. **Schedule** (Default button - Grey)
   - Icon: Schedule/clock icon
   - Action: Opens prompt for date/time input
   - Sets PublishAt timestamp
   - Note: Requires background job to auto-publish (not implemented)

3. **Keep as Draft** (Text button)
   - Icon: Save icon
   - Action: Shows confirmation toast
   - No actual changes to database

4. **Delete** (Warn button - Red)
   - Icon: Delete/trash icon
   - Action: Shows confirmation dialog, then deletes
   - Permanent deletion from database

### Empty State
If no drafts exist:
- Large inbox icon (64px)
- Heading: "No draft content available."
- Centered on the page

## Responsive Behavior

### Desktop (> 768px)
- Multi-column grid (2-3 columns depending on screen width)
- Horizontal action buttons

### Mobile (≤ 768px)
- Single column layout
- Stacked action buttons (full width)
- Reduced padding

## User Interactions

### Publishing a Draft
1. Admin clicks "Publish Now" button
2. API call: `PUT /api/cms/content/{id}` with `{ draft: false, active: true }`
3. Success toast: "Content published successfully"
4. Page reloads to show updated draft list
5. Content now appears on public blog page

### Scheduling Publication
1. Admin clicks "Schedule" button
2. Browser prompt: "Enter publication date and time (YYYY-MM-DD HH:MM):"
3. Admin enters date (e.g., "2026-01-15 14:00")
4. API call: `PUT /api/cms/content/{id}` with `{ publishAt: "2026-01-15T14:00:00Z" }`
5. Success toast: "Publication scheduled successfully"
6. Card updates to show scheduled date

### Deleting a Draft
1. Admin clicks "Delete" button
2. Confirmation dialog: "Are you sure you want to delete '[Title]'?"
3. Admin confirms
4. API call: `DELETE /api/cms/content/{id}`
5. Success toast: "Draft deleted successfully"
6. Card removed from view

## Color Scheme

- **Primary Color**: Blue (#3f51b5 - Material Design Blue)
- **Accent Color**: Orange/Yellow for badges
- **Warn Color**: Red for delete button
- **Background**: Light grey (#f5f5f5) for content preview
- **Text**: Dark grey (#333) for primary text
- **Secondary Text**: Medium grey (#666) for metadata

## Material Components Used

- `mat-card` - Card container
- `mat-card-header` - Card title area
- `mat-card-content` - Main content area
- `mat-card-actions` - Action buttons area
- `mat-button` / `mat-raised-button` - Action buttons
- `mat-icon` - Material icons
- `mat-chip` - Draft badge
- `mat-spinner` - Loading indicator
- `mat-snack-bar` - Toast notifications

## Example Draft Card

```
┌─────────────────────────────────────────────┐
│ Getting Started with Angular                │
│ A comprehensive guide for beginners         │
│                                             │
│ 👤 John Doe                                 │
│ 📅 Created: Dec 29, 2025, 10:00 AM        │
│ ⏰ Scheduled: Jan 2, 2026, 3:00 PM        │
│                                             │
│ ┌─────────────────────────────────────┐   │
│ │ # Introduction                       │   │
│ │                                      │   │
│ │ Angular is a powerful framework...  │   │
│ └─────────────────────────────────────┘   │
│                                             │
│ [DRAFT]                                     │
│                                             │
│ ─────────────────────────────────────────  │
│ [📤 Publish Now] [⏰ Schedule]             │
│ [💾 Keep as Draft] [🗑️ Delete]            │
└─────────────────────────────────────────────┘
```

## API Integration

### Fetching Drafts
```typescript
this.cmsService.getDraftContent().subscribe({
  next: (data) => {
    this.drafts = data;
    this.loading = false;
  },
  error: (error) => {
    this.error = 'Failed to load draft content';
    this.snackBar.open('Failed to load drafts', 'Close', { duration: 3000 });
  }
});
```

### Publishing Draft
```typescript
this.cmsService.updateContent(draft.id, { 
  draft: false, 
  active: true 
}).subscribe({
  next: () => {
    this.snackBar.open('Content published successfully', 'Close', { duration: 3000 });
    this.loadDrafts(); // Refresh list
  }
});
```

### Deleting Draft
```typescript
this.cmsService.deleteContent(draft.id).subscribe({
  next: () => {
    this.snackBar.open('Draft deleted successfully', 'Close', { duration: 3000 });
    this.loadDrafts(); // Refresh list
  }
});
```

## Security

- Route protected by `AuthGuard` and `AdminGuard`
- Redirects to login if not authenticated
- Redirects to home if authenticated but not admin
- All API calls include JWT token automatically via interceptor
- Backend validates admin role on every request

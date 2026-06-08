import { Component, OnInit, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ShowingService, ShowingGroupedRoom, ShowingItemRoom } from '../../../../core/services/admin-theater/showing.service';
import { RoomService } from '../../../../core/services/admin-theater/room.service';
import { TheaterShowtimesFormComponent } from '../form/showtimes-form.component';

@Component({
  selector: 'app-theater-showtimes-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TheaterShowtimesFormComponent],
  templateUrl: './showtimes-list.component.html'
})
export class TheaterShowtimesListComponent implements OnInit {
  private showingService = inject(ShowingService);
  private roomService = inject(RoomService);

  groupedRooms = signal<ShowingGroupedRoom[]>([]);
  rooms = signal<any[]>([]); // room filter list
  
  totalCount = signal(0);
  pageIndex = signal(1);
  pageSize = signal(10);
  loading = signal(false);

  // Filters
  filterDate = signal<string>(''); // YYYY-MM-DD
  selectedRoomId = signal<string>(''); // "" means all rooms
  filterActive = signal<boolean | undefined>(undefined);

  // Modal form states
  showForm = signal(false);
  selectedShowingId = signal<string | null>(null);

  // Timeline dynamic hours ruler
  timelineHours = signal<string[]>([]);
  minHour = signal<number>(8);
  maxHour = signal<number>(22);

  hourWidth = 120; // width of each hour in px

  constructor() {
    // Automatically reload showings when filters change
    effect(() => {
      this.loadData();
    }, { allowSignalWrites: true });
  }

  ngOnInit(): void {
    // Set default filter date to today
    const todayStr = new Date().toISOString().split('T')[0];
    this.filterDate.set(todayStr);

    // Load rooms list for dropdown filter
    this.roomService.getRoomsByTheater().subscribe({
      next: (res) => {
        this.rooms.set(res);
      },
      error: (err) => {
        console.error('Error fetching rooms for filter:', err);
      }
    });
  }

  loadData(): void {
    this.loading.set(true);
    this.showingService.getShowingsGroupedByRoom({
      dateStart: this.filterDate() || undefined,
      roomId: this.selectedRoomId() || undefined,
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
      isActive: this.filterActive()
    }).subscribe({
      next: (res) => {
        this.groupedRooms.set(res.items);
        this.totalCount.set(res.totalCount);

        // Calculate dynamic timeline bounds based on showings
        let earliest = 24 * 60; // large value representing end of day
        let latest = 0;
        let hasShowings = false;

        res.items.forEach(room => {
          if (room.showings && room.showings.length > 0) {
            room.showings.forEach(s => {
              hasShowings = true;
              
              // Start time in adjusted minutes
              const start = new Date(s.timeStart);
              let startH = start.getHours();
              if (startH < 4) startH += 24;
              const startMin = startH * 60 + start.getMinutes();
              if (startMin < earliest) earliest = startMin;

              // End time in adjusted minutes
              let endH = 0;
              let endM = 0;
              const end = new Date(s.timeEnd);
              if (end && end.getFullYear() > 1970) {
                endH = end.getHours();
                if (endH < 4) endH += 24;
                endM = end.getMinutes();
              } else {
                // Default fallback: start time + 120 minutes (2 hours)
                endH = startH + 2;
                endM = start.getMinutes();
              }
              const endMin = endH * 60 + endM;
              if (endMin > latest) latest = endMin;
            });
          }
        });

        if (hasShowings) {
          // Floor the earliest minutes to the previous hour boundary
          const minH = Math.floor(earliest / 60);
          // Ceil the latest minutes to the next hour boundary
          const maxH = Math.ceil(latest / 60);
          
          this.minHour.set(Math.max(0, minH - 1)); // 1-hour margin before earliest show
          this.maxHour.set(Math.min(28, maxH + 1)); // 1-hour margin after latest show
        } else {
          // Fallback static range when no showings exist
          this.minHour.set(8);
          this.maxHour.set(22);
        }

        // Generate the formatted hours array for the timeline ruler
        const hoursList: string[] = [];
        const start = this.minHour();
        const end = this.maxHour();
        for (let h = start; h <= end; h++) {
          const displayHour = h % 24;
          hoursList.push(String(displayHour).padStart(2, '0') + ':00');
        }
        this.timelineHours.set(hoursList);

        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching room-grouped showings:', err);
        this.groupedRooms.set([]);
        this.totalCount.set(0);
        this.loading.set(false);
      }
    });
  }

  // Calculate coordinates in the timeline
  getShowingLeft(timeStartStr: string): number {
    const date = new Date(timeStartStr);
    let hours = date.getHours();
    const minutes = date.getMinutes();

    // Map late night/early morning hours (00:00 - 03:59) to 24-27
    if (hours < 4) {
      hours += 24;
    }

    const startMinutes = hours * 60 + minutes;
    const timelineStartMinutes = this.minHour() * 60; // timeline starts at minHour
    const diff = startMinutes - timelineStartMinutes;

    const left = (diff / 60) * this.hourWidth;
    return Math.max(0, left);
  }

  getShowingWidth(timeStartStr: string, timeEndStr: string): number {
    const timeStart = new Date(timeStartStr);
    const timeEnd = new Date(timeEndStr);

    let durationMinutes = 120; // fallback if no valid end time
    if (timeEnd && timeEnd.getFullYear() > 1970) {
      const diffMs = timeEnd.getTime() - timeStart.getTime();
      durationMinutes = Math.floor(diffMs / (60 * 1000));
    }

    const width = (durationMinutes / 60) * this.hourWidth;
    return Math.max(90, width); // minimum width for readability
  }

  formatTimeOnly(dateTimeStr: string): string {
    if (!dateTimeStr || dateTimeStr.startsWith('0001')) return '--:--';
    const date = new Date(dateTimeStr);
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${hours}:${minutes}`;
  }

  onDateChange(dateStr: string): void {
    this.filterDate.set(dateStr);
    this.pageIndex.set(1);
  }

  onRoomChange(roomId: string): void {
    this.selectedRoomId.set(roomId);
    this.pageIndex.set(1);
  }

  onActiveFilterChange(val: boolean | undefined): void {
    this.filterActive.set(val);
    this.pageIndex.set(1);
  }

  onPageChange(page: number): void {
    this.pageIndex.set(page);
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageIndex.set(1);
  }

  totalPages(): number {
    return Math.ceil(this.totalCount() / this.pageSize());
  }

  getPagesArray(): number[] {
    const total = this.totalPages();
    const arr: number[] = [];
    for (let i = 1; i <= total; i++) {
      arr.push(i);
    }
    return arr;
  }

  openCreate(): void {
    this.selectedShowingId.set(null);
    this.showForm.set(true);
  }

  onEdit(showing: ShowingItemRoom): void {
    this.selectedShowingId.set(showing.id);
    this.showForm.set(true);
  }

  onToggleActive(showing: ShowingItemRoom, event: Event): void {
    event.stopPropagation(); // prevent opening the edit dialog
    this.showingService.updateShowing(showing.id, { isActive: !showing.isActive }).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        console.error('Error toggling active status:', err);
        alert('Có lỗi xảy ra khi cập nhật trạng thái lịch chiếu.');
      }
    });
  }

  onFormClose(saved: boolean): void {
    this.showForm.set(false);
    if (saved) {
      this.loadData();
    }
  }
}

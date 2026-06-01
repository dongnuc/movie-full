import { Component, OnInit, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ShowingService, Showing } from '../../../../core/services/showing.service';
import { TheaterService, Theater } from '../../../../core/services/theater.service';
import { AdminTableComponent } from '../../../../shared/components/admin-table/admin-table.component';
import { TheaterShowtimesFormComponent } from '../form/showtimes-form.component';
import { TableColumn } from '../../../../shared/models/table-config';

@Component({
  selector: 'app-theater-showtimes-list',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminTableComponent, TheaterShowtimesFormComponent],
  templateUrl: './showtimes-list.component.html'
})
export class TheaterShowtimesListComponent implements OnInit {
  private showingService = inject(ShowingService);
  private theaterService = inject(TheaterService);

  showings = signal<Showing[]>([]);
  theaters = signal<Theater[]>([]);
  selectedTheaterId = signal<string>('');
  
  totalCount = signal(0);
  pageIndex = signal(1);
  pageSize = signal(10);
  loading = signal(false);

  // Filters
  filterDate = signal<string>(''); // YYYY-MM-DD
  filterActive = signal<boolean | undefined>(undefined);

  // Modal form states
  showForm = signal(false);
  selectedShowingId = signal<string | null>(null);

  columns: TableColumn[] = [
    { key: 'movieTitle', header: 'Phim Chiếu', type: 'text' },
    { key: 'roomName', header: 'Phòng Chiếu', type: 'text' },
    { key: 'timeStart', header: 'Giờ Chiếu', type: 'date', dateFormat: 'dd/MM/yyyy HH:mm' },
    { key: 'price', header: 'Giá Vé Cơ Bản', type: 'currency' },
    { key: 'isActive', header: 'Trạng Thái', type: 'boolean' }
  ];

  constructor() {
    // Automatically reload showings when theater or filters change
    effect(() => {
      if (this.selectedTheaterId()) {
        this.loadData();
      }
    });
  }

  ngOnInit(): void {
    // Set default filter date to today
    const todayStr = new Date().toISOString().split('T')[0];
    this.filterDate.set(todayStr);

    this.loadTheaters();
  }

  loadTheaters(): void {
    this.theaterService.getTheaters({ page: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        this.theaters.set(res.items);
        const storedTheaterId = localStorage.getItem('owner_selected_theater_id');
        
        if (storedTheaterId && res.items.some(t => t.id === storedTheaterId)) {
          this.selectedTheaterId.set(storedTheaterId);
        } else if (res.items.length > 0) {
          this.selectedTheaterId.set(res.items[0].id);
        }
      }
    });
  }

  loadData(): void {
    if (!this.selectedTheaterId()) return;

    this.loading.set(true);
    this.showingService.getShowingsByTheater(this.selectedTheaterId(), {
      dateStart: this.filterDate() || undefined,
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
      isActive: this.filterActive()
    }).subscribe({
      next: (res) => {
        // Flatten grouped response
        const flatList: Showing[] = [];
        res.items.forEach(group => {
          if (group.showings && group.showings.length > 0) {
            group.showings.forEach(show => {
              flatList.push({
                ...show,
                movieTitle: group.movieTitle,
                movieDuration: show.movieDuration
              });
            });
          }
        });
        
        this.showings.set(flatList);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching showings:', err);
        this.showings.set([]);
        this.totalCount.set(0);
        this.loading.set(false);
      }
    });
  }

  onTheaterChange(theaterId: string): void {
    this.selectedTheaterId.set(theaterId);
    localStorage.setItem('owner_selected_theater_id', theaterId);
    this.pageIndex.set(1);
  }

  onDateChange(dateStr: string): void {
    this.filterDate.set(dateStr);
    this.pageIndex.set(1);
  }

  onActiveFilterChange(event: any): void {
    const val = event.target.value;
    this.filterActive.set(val === 'true' ? true : val === 'false' ? false : undefined);
    this.pageIndex.set(1);
    this.loadData();
  }

  onPageChange(page: number): void {
    this.pageIndex.set(page);
    this.loadData();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageIndex.set(1);
    this.loadData();
  }

  openCreate(): void {
    if (!this.selectedTheaterId()) {
      alert('Vui lòng chọn rạp chiếu trước.');
      return;
    }
    this.selectedShowingId.set(null);
    this.showForm.set(true);
  }

  onEdit(row: Showing): void {
    this.selectedShowingId.set(row.id);
    this.showForm.set(true);
  }

  onToggleActive(row: Showing): void {
    this.showingService.updateShowing(row.id, { isActive: !row.isActive, roomId: row.roomId }).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        console.error('Error toggling showing active status:', err);
        alert('Có lỗi xảy ra khi đổi trạng thái suất chiếu.');
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

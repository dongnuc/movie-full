import { Component, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

interface MovieItem {
  id: string;
  title: string;
  image: string;
  duration: number;
  categoryName: string;
  description: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html'
})
export class HomeComponent {
  authService = inject(AuthService);
  private router = inject(Router);

  isScrolled = signal(false);

  // Seed local movie mocks for layout display prior to API hooking
  playingMovies: MovieItem[] = [
    {
      id: 'e3df8d32-26d9-4828-98e3-ea4cf92c55b1',
      title: 'Dune: Part Two',
      image: 'https://image.tmdb.org/t/p/w500/8b8me1hJu524Zg3KRas4hdv5tZh.jpg',
      duration: 166,
      categoryName: 'Khoa Học Viễn Tưởng',
      description: 'Paul Atreides unites with Chani and the Fremen while seeking revenge.'
    },
    {
      id: 'fa4f1a26-9d3b-483d-9d41-4770e0600123',
      title: 'Inside Out 2',
      image: 'https://image.tmdb.org/t/p/w500/vpnVM1B655z4x6P5o4xZ5tjn2zs.jpg',
      duration: 96,
      categoryName: 'Hoạt Hình',
      description: 'Follow Riley as she enters teenage years and encounters new emotions.'
    },
    {
      id: 'd9b048ac-08b2-4d43-982d-209fe16fcdff',
      title: 'Oppenheimer',
      image: 'https://image.tmdb.org/t/p/w500/8Gxv2Z7Hkug64JpkPLtg6ugRzsb.jpg',
      duration: 180,
      categoryName: 'Tiểu Sử / Lịch Sử',
      description: 'The story of J. Robert Oppenheimer and the Manhattan Project.'
    },
    {
      id: 'cb6da8ac-d2a9-4cb5-ae0d-d4eb723ff411',
      title: 'Kung Fu Panda 4',
      image: 'https://image.tmdb.org/t/p/w500/kDp1vUBi3212xSN3kOAJZg6RmqZ.jpg',
      duration: 94,
      categoryName: 'Hoạt Hình / Hài',
      description: 'Po is tapped to become the Spiritual Leader of the Valley of Peace.'
    }
  ];

  upcomingMovies: MovieItem[] = [
    {
      id: '992a2cb5-d2ae-443b-be2f-2d4e723ff123',
      title: 'Deadpool & Wolverine',
      image: 'https://image.tmdb.org/t/p/w500/8cdWj6RFOYxhOUebz7WEx624tzy.jpg',
      duration: 127,
      categoryName: 'Hành Động',
      description: 'Wolverine is recovering from his injuries when he crosses paths with Deadpool.'
    },
    {
      id: 'aa1a2cb5-e3d1-443b-be2f-2d4e723ff334',
      title: 'Moana 2',
      image: 'https://image.tmdb.org/t/p/w500/2LgP1wJ3J5D3jQ4K1X3RmgzP7f.jpg',
      duration: 100,
      categoryName: 'Hoạt Hình / Nhạc Kịch',
      description: 'Moana receives an unexpected call from her wayfinding ancestors.'
    }
  ];

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.isScrolled.set(window.scrollY > 50);
  }

  logout(): void {
    this.authService.logout().subscribe(() => {
      this.router.navigate(['/auth/login']);
    });
  }
}

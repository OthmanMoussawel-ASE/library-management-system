import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CheckoutService } from '../../core/services/checkout.service';
import { AIService, RecommendedBook, BookRecommendationsResponse } from '../../core/services/ai.service';
import { AuthService } from '../../core/services/auth.service';
import { DashboardStats } from '../../core/models/checkout.model';
import { CheckoutRecord } from '../../core/models/checkout.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatIconModule,
    MatListModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private checkoutService = inject(CheckoutService);
  private aiService = inject(AIService);
  private authService = inject(AuthService);

  stats: DashboardStats | null = null;
  libraryRecommendations: RecommendedBook[] = [];
  discoverMoreRecommendations: string[] = [];
  recentCheckouts: CheckoutRecord[] = [];
  aiAvailable = false;
  isLoadingStats = true;
  isLoadingRecommendations = false;
  isLoadingRecent = true;

  get currentUserName(): string {
    return this.authService.currentUser?.fullName ?? 'User';
  }

  get isStaff(): boolean {
    return this.authService.isLibrarian;
  }

  ngOnInit(): void {
    this.loadStats();
    this.loadRecentActivity();
    this.checkAIAndLoadRecommendations();
  }

  private loadStats(): void {
    this.isLoadingStats = true;
    this.checkoutService.getDashboardStats().subscribe({
      next: (stats) => {
        this.stats = stats;
        this.isLoadingStats = false;
      },
      error: () => {
        this.isLoadingStats = false;
      },
    });
  }

  private loadRecentActivity(): void {
    this.isLoadingRecent = true;
    this.checkoutService
      .getCheckouts({
        pageNumber: 1,
        pageSize: 5,
        sortBy: 'checkedOutAt',
        sortDirection: 'desc',
      })
      .subscribe({
        next: (result) => {
          this.recentCheckouts = result.items;
          this.isLoadingRecent = false;
        },
        error: () => {
          this.isLoadingRecent = false;
        },
      });
  }

  private checkAIAndLoadRecommendations(): void {
    this.aiService.getStatus().subscribe({
      next: (res) => {
        this.aiAvailable = res.available;
        if (this.aiAvailable) {
          this.loadRecommendations();
        }
      },
      error: () => {
        this.aiAvailable = false;
      },
    });
  }

  private loadRecommendations(): void {
    this.isLoadingRecommendations = true;
    this.aiService.getRecommendations().subscribe({
      next: (response) => {
        this.libraryRecommendations = response?.fromLibrary ?? [];
        this.discoverMoreRecommendations = response?.discoverMore ?? [];
        this.isLoadingRecommendations = false;
      },
      error: () => {
        this.isLoadingRecommendations = false;
      },
    });
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }
}

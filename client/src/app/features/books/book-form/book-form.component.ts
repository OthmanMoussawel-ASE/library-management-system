import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { forkJoin } from 'rxjs';
import {
  CreateBookRequest,
  UpdateBookRequest,
  Author,
  Category,
} from '../../../core/models/book.model';
import { BookService } from '../../../core/services/book.service';

@Component({
  selector: 'app-book-form',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatTooltipModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule,
  ],
  templateUrl: './book-form.component.html',
  styleUrl: './book-form.component.scss',
})
export class BookFormComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private bookService = inject(BookService);
  private snackBar = inject(MatSnackBar);

  form!: FormGroup;
  authors: Author[] = [];
  categories: Category[] = [];
  isEditMode = false;
  bookId: string | null = null;
  isLoading = false;
  isSaving = false;

  showNewAuthor = false;
  newAuthorFirstName = '';
  newAuthorLastName = '';
  newAuthorBio = '';
  isSavingAuthor = false;

  showNewCategory = false;
  newCategoryName = '';
  newCategoryDesc = '';
  isSavingCategory = false;

  isGeneratingDescription = false;
  isSuggestingCategories = false;

  suggestedNewCategories: string[] = [];

  ngOnInit(): void {
    this.bookId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.bookId;

    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      isbn: ['', [Validators.maxLength(20), Validators.pattern(/^[\d\-]*$/)]],
      description: ['', Validators.maxLength(2000)],
      coverImageUrl: ['', [Validators.maxLength(500), Validators.pattern(/^$|^https?:\/\/.+/)]],
      totalCopies: [1, [Validators.required, Validators.min(1), Validators.max(1000), Validators.pattern(/^\d+$/)]],
      publishedDate: [null as Date | null],
      publisher: ['', Validators.maxLength(200)],
      pageCount: [null as number | null, [Validators.min(1), Validators.max(10000), Validators.pattern(/^\d*$/)]],
      language: ['', Validators.maxLength(50)],
      authorId: ['', Validators.required],
      categoryIds: [[] as string[]],
    });

    const requests: {
      authors: ReturnType<BookService['getAuthors']>;
      categories: ReturnType<BookService['getCategories']>;
      book?: ReturnType<BookService['getBook']>;
    } = {
      authors: this.bookService.getAuthors(),
      categories: this.bookService.getCategories(),
    };

    if (this.isEditMode && this.bookId) {
      requests.book = this.bookService.getBook(this.bookId);
    }

    this.isLoading = true;
    forkJoin(requests).subscribe({
      next: ({ authors, categories, book }) => {
        this.authors = authors;
        this.categories = categories;
        if (book) {
          this.form.patchValue({
            title: book.title,
            isbn: book.isbn ?? '',
            description: book.description ?? '',
            coverImageUrl: book.coverImageUrl ?? '',
            totalCopies: book.totalCopies,
            publishedDate: book.publishedDate ? new Date(book.publishedDate) : null,
            publisher: book.publisher ?? '',
            pageCount: book.pageCount ?? null,
            language: book.language ?? '',
            authorId: book.authorId,
            categoryIds: this.resolveCategoryIds(book.categories),
          });
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.snackBar.open(
          this.isEditMode ? 'Book not found' : 'Failed to load form data',
          'Close',
          {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top',
          }
        );
        if (this.isEditMode) {
          this.router.navigate(['/books']);
        }
      },
    });
  }

  private resolveCategoryIds(categoryNames: string[]): string[] {
    return categoryNames
      .map((name) => this.categories.find((c) => c.name === name)?.id)
      .filter((id): id is string => !!id);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    const formatDate = (d: Date | null): string | undefined =>
      d ? d.toISOString().split('T')[0] : undefined;

    if (this.isEditMode && this.bookId) {
      const request: UpdateBookRequest = {
        title: value.title,
        isbn: value.isbn || undefined,
        description: value.description || undefined,
        coverImageUrl: value.coverImageUrl || undefined,
        totalCopies: value.totalCopies,
        publishedDate: formatDate(value.publishedDate),
        publisher: value.publisher || undefined,
        pageCount: value.pageCount ?? undefined,
        language: value.language || undefined,
        authorId: value.authorId,
        categoryIds: value.categoryIds?.length ? value.categoryIds : undefined,
      };
      this.isSaving = true;
      this.bookService.updateBook(this.bookId, request).subscribe({
        next: () => {
          this.snackBar.open('Book updated successfully', 'Close', {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top',
          });
          this.router.navigate(['/books', this.bookId]);
        },
        error: (err) => {
          this.isSaving = false;
          this.snackBar.open(
            err?.error?.message || 'Update failed',
            'Close',
            {
              duration: 5000,
              horizontalPosition: 'end',
              verticalPosition: 'top',
            }
          );
        },
      });
    } else {
      const request: CreateBookRequest = {
        ...value,
        isbn: value.isbn || undefined,
        description: value.description || undefined,
        coverImageUrl: value.coverImageUrl || undefined,
        publishedDate: formatDate(value.publishedDate),
        publisher: value.publisher || undefined,
        pageCount: value.pageCount ?? undefined,
        language: value.language || undefined,
        categoryIds: value.categoryIds?.length ? value.categoryIds : undefined,
      };
      this.isSaving = true;
      this.bookService.createBook(request).subscribe({
        next: (book) => {
          this.snackBar.open('Book created successfully', 'Close', {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top',
          });
          this.router.navigate(['/books', book.id]);
        },
        error: (err) => {
          this.isSaving = false;
          this.snackBar.open(
            err?.error?.message || 'Create failed',
            'Close',
            {
              duration: 5000,
              horizontalPosition: 'end',
              verticalPosition: 'top',
            }
          );
        },
      });
    }
  }

  addAuthor(): void {
    if (!this.newAuthorFirstName.trim() || !this.newAuthorLastName.trim()) return;
    this.isSavingAuthor = true;
    this.bookService
      .createAuthor({
        firstName: this.newAuthorFirstName.trim(),
        lastName: this.newAuthorLastName.trim(),
        biography: this.newAuthorBio.trim() || undefined,
      })
      .subscribe({
        next: (author) => {
          this.authors = [...this.authors, author];
          this.form.patchValue({ authorId: author.id });
          this.showNewAuthor = false;
          this.newAuthorFirstName = '';
          this.newAuthorLastName = '';
          this.newAuthorBio = '';
          this.isSavingAuthor = false;
          this.snackBar.open('Author created', 'Close', { duration: 2000, horizontalPosition: 'end', verticalPosition: 'top' });
        },
        error: () => {
          this.isSavingAuthor = false;
          this.snackBar.open('Failed to create author', 'Close', { duration: 3000, horizontalPosition: 'end', verticalPosition: 'top' });
        },
      });
  }

  addCategory(): void {
    if (!this.newCategoryName.trim()) return;
    this.isSavingCategory = true;
    this.bookService
      .createCategory({
        name: this.newCategoryName.trim(),
        description: this.newCategoryDesc.trim() || undefined,
      })
      .subscribe({
        next: (category) => {
          this.categories = [...this.categories, category];
          const current: string[] = this.form.get('categoryIds')?.value ?? [];
          this.form.patchValue({ categoryIds: [...current, category.id] });
          this.showNewCategory = false;
          this.newCategoryName = '';
          this.newCategoryDesc = '';
          this.isSavingCategory = false;
          this.snackBar.open('Category created', 'Close', { duration: 2000, horizontalPosition: 'end', verticalPosition: 'top' });
        },
        error: () => {
          this.isSavingCategory = false;
          this.snackBar.open('Failed to create category', 'Close', { duration: 3000, horizontalPosition: 'end', verticalPosition: 'top' });
        },
      });
  }

  generateDescription(): void {
    const title = this.form.get('title')?.value?.trim();
    const authorId = this.form.get('authorId')?.value;
    const author = this.authors.find(a => a.id === authorId);

    if (!title || !author) {
      this.snackBar.open('Please enter a title and select an author first', 'Close', {
        duration: 3000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
      return;
    }

    this.isGeneratingDescription = true;
    this.bookService.generateDescription(title, author.fullName).subscribe({
      next: (result) => {
        this.form.patchValue({ description: result.description });
        this.isGeneratingDescription = false;
        this.snackBar.open('Description generated', 'Close', {
          duration: 2000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      },
      error: () => {
        this.isGeneratingDescription = false;
        this.snackBar.open('Failed to generate description', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      },
    });
  }

  suggestCategories(): void {
    const title = this.form.get('title')?.value?.trim();
    const authorId = this.form.get('authorId')?.value;
    const description = this.form.get('description')?.value?.trim();
    const author = this.authors.find(a => a.id === authorId);

    if (!title || !author) {
      this.snackBar.open('Please enter a title and select an author first', 'Close', {
        duration: 3000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
      return;
    }

    this.isSuggestingCategories = true;
    this.suggestedNewCategories = [];
    
    this.bookService.suggestCategories(title, author.fullName, description).subscribe({
      next: (result) => {
        const matchedIds = result.existing
          .map(name => this.categories.find(c => c.name.toLowerCase() === name.toLowerCase())?.id)
          .filter((id): id is string => !!id);

        if (matchedIds.length > 0) {
          this.form.patchValue({ categoryIds: matchedIds });
        }

        this.suggestedNewCategories = result.suggested;

        const messages: string[] = [];
        if (matchedIds.length > 0) {
          messages.push(`${matchedIds.length} existing categories selected`);
        }
        if (result.suggested.length > 0) {
          messages.push(`${result.suggested.length} new categories suggested`);
        }

        this.snackBar.open(messages.join(', ') || 'No categories suggested', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
        this.isSuggestingCategories = false;
      },
      error: () => {
        this.isSuggestingCategories = false;
        this.snackBar.open('Failed to suggest categories', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      },
    });
  }

  createSuggestedCategory(name: string): void {
    this.bookService.createCategory({ name }).subscribe({
      next: (newCategory) => {
        this.categories = [...this.categories, newCategory];
        const currentIds = this.form.get('categoryIds')?.value || [];
        this.form.patchValue({ categoryIds: [...currentIds, newCategory.id] });
        this.suggestedNewCategories = this.suggestedNewCategories.filter(c => c !== name);
        this.snackBar.open(`Category "${name}" created and selected`, 'Close', {
          duration: 2000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      },
      error: () => {
        this.snackBar.open(`Failed to create category "${name}"`, 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      },
    });
  }

  dismissSuggestedCategory(name: string): void {
    this.suggestedNewCategories = this.suggestedNewCategories.filter(c => c !== name);
  }
}

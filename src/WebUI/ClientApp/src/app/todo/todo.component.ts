import { Component, TemplateRef, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import {
  TodoListsClient,
  TodoItemsClient,
  TodoListDto,
  TodoItemDto,
  PriorityLevelDto,
  CreateTodoListCommand,
  UpdateTodoListCommand,
  CreateTodoItemCommand,
  UpdateTodoItemDetailCommand,
} from '../web-api-client';

@Component({
  selector: 'app-todo-component',
  templateUrl: './todo.component.html',
  styleUrls: ['./todo.component.scss'],
})
export class TodoComponent implements OnInit {
  debug = false;
  deleting = false;
  deleteCountDown = 0;
  deleteCountDownInterval: any;
  lists: TodoListDto[];
  priorityLevels: PriorityLevelDto[];
  selectedList: TodoListDto;
  selectedItem: TodoItemDto;
  availableTags: string[] = [];
  newTag: string = '';
  filterTags: string[] = [];
  popularTags: string[] = [];
  unfilteredLists: TodoListDto[] = [];
  deletedLists: TodoListDto[] = [];
  deletedItems: TodoItemDto[] = [];
  showDeleted: boolean = false;
  searchTerm: string = '';
  private searchDebounceTimer: any;
  newListEditor: any = {};
  listOptionsEditor: any = {};
  newListModalRef: BsModalRef;
  listOptionsModalRef: BsModalRef;
  deleteListModalRef: BsModalRef;
  itemDetailsModalRef: BsModalRef;
  itemDetailsFormGroup = this.fb.group({
    id: [null],
    listId: [null],
    priority: [''],
    note: [''],
    backgroundColor: ['#ffffff'],
    tags: [[]],
  });

  constructor(
    private listsClient: TodoListsClient,
    private itemsClient: TodoItemsClient,
    private modalService: BsModalService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.loadTodoLists();
    this.loadAvailableTags();
  }

  ngOnDestroy(): void {
    if (this.searchDebounceTimer) {
      clearTimeout(this.searchDebounceTimer);
    }
  }

  // Lists
  remainingItems(list: TodoListDto): number {
    return list.items.filter((t) => !t.done).length;
  }

  showNewListModal(template: TemplateRef<any>): void {
    this.newListModalRef = this.modalService.show(template);
    setTimeout(() => document.getElementById('title').focus(), 250);
  }

  newListCancelled(): void {
    this.newListModalRef.hide();
    this.newListEditor = {};
  }

  addList(): void {
    const list = {
      id: 0,
      title: this.newListEditor.title,
      items: [],
    } as TodoListDto;

    this.listsClient.create(list as CreateTodoListCommand).subscribe(
      (result) => {
        list.id = result;
        this.lists.push(list);
        this.selectedList = list;
        this.newListModalRef.hide();
        this.newListEditor = {};
      },
      (error) => {
        const errors = JSON.parse(error.response);

        if (errors && errors.Title) {
          this.newListEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('title').focus(), 250);
      }
    );
  }

  showListOptionsModal(template: TemplateRef<any>) {
    this.listOptionsEditor = {
      id: this.selectedList.id,
      title: this.selectedList.title,
    };

    this.listOptionsModalRef = this.modalService.show(template);
  }

  updateListOptions() {
    const list = this.listOptionsEditor as UpdateTodoListCommand;
    this.listsClient.update(this.selectedList.id, list).subscribe(
      () => {
        (this.selectedList.title = this.listOptionsEditor.title),
          this.listOptionsModalRef.hide();
        this.listOptionsEditor = {};
      },
      (error) => console.error(error)
    );
  }

  confirmDeleteList(template: TemplateRef<any>) {
    this.listOptionsModalRef.hide();
    this.deleteListModalRef = this.modalService.show(template);
  }

  deleteListConfirmed(): void {
    this.listsClient.delete(this.selectedList.id).subscribe({
      next: () => {
        this.deleteListModalRef.hide();
        this.lists = this.lists.filter((t) => t.id !== this.selectedList.id);

        this.selectedList = this.lists.length ? this.lists[0] : null;

        console.log('List moved to trash');
      },
      error: (error) => {
        console.error('Error deleting list', error);
      },
    });
  }

  // Items
  showItemDetailsModal(template: TemplateRef<any>, item: TodoItemDto): void {
    this.selectedItem = item;
    this.itemDetailsFormGroup.patchValue({
      ...item,
      tags: item.tags || [],
    });

    this.loadAvailableTags();
    console.log(this.lists);

    this.itemDetailsModalRef = this.modalService.show(template);
    this.itemDetailsModalRef.onHidden.subscribe(() => {
      this.stopDeleteCountDown();
    });
  }

  loadAvailableTags(): void {
    this.itemsClient.getAvailableTags().subscribe({
      next: (tags: string[]) => {
        const usedTags = this.getUsedTags();
        this.availableTags = tags.filter(
          (tag) => typeof tag === 'string' && usedTags.includes(tag)
        );

        this.popularTags = this.getPopularTags();

        this.filterTags = this.filterTags.filter(
          (tag) => typeof tag === 'string' && this.availableTags.includes(tag)
        );
      },
      error: (error) => console.error('Error loading tags:', error),
    });
  }

  private getUsedTags(): string[] {
    if (!this.selectedList || !this.selectedList.items) return [];

    const allTags = this.selectedList.items
      .filter((item) => !item.isDeleted)
      .map((item) => item.tags || [])
      .reduce((acc, tags) => acc.concat(tags), [])
      .filter((tag) => typeof tag === 'string' && tag.trim() !== '');

    return Array.from(new Set(allTags));
  }

  private getPopularTags(): string[] {
    if (!this.selectedList || !this.selectedList.items) return [];

    const tagCounts: { [tag: string]: number } = {};

    this.selectedList.items
      .filter((item) => !item.isDeleted)
      .forEach((item) => {
        item.tags?.forEach((tag) => {
          if (typeof tag === 'string' && tag.trim() !== '') {
            tagCounts[tag] = (tagCounts[tag] || 0) + 1;
          }
        });
      });

    return Object.entries(tagCounts)
      .sort((a, b) => b[1] - a[1])
      .map(([tag]) => tag)
      .slice(0, 5);
  }

  updateItemDetails(): void {
    const formValue = this.itemDetailsFormGroup.value;
    const command = new UpdateTodoItemDetailCommand({
      ...formValue,
      tags: formValue.tags || [],
    });

    this.itemsClient
      .updateItemDetails(this.selectedItem.id, command)
      .subscribe({
        next: () => {
          Object.assign(this.selectedItem, {
            priority: formValue.priority,
            note: formValue.note,
            backgroundColor: formValue.backgroundColor,
            tags: [...formValue.tags],
          });

          this.itemDetailsModalRef.hide();

          this.refreshTodoList();
        },
        error: (error) => {
          console.error('Update Error:', error);
        },
      });
  }

  refreshTodoList(): void {
    this.listsClient.get().subscribe({
      next: (result) => {
        const lists = result.lists.map((list) =>
          Object.assign(new TodoListDto(), {
            ...list,
            items: list.items.map((item) =>
              Object.assign(new TodoItemDto(), {
                ...item,
                backgroundColor: item.backgroundColor || '#ffffff',
                tags: item.tags || [],
              })
            ),
          })
        );

        this.unfilteredLists = lists;
        this.lists = [...lists];

        if (this.selectedList) {
          this.selectedList =
            this.lists.find((l) => l.id === this.selectedList.id) ||
            this.lists[0];
        }
      },
      error: (error) => console.error('Fetching Data Error:', error),
    });
  }

  onListSelected(list: TodoListDto): void {
    this.selectedList = list;
    this.filterTags = [];
    this.loadAvailableTags();
    this.applyFilters();
  }

  addItem() {
    const defaultPriority =
      this.priorityLevels?.length > 0 ? this.priorityLevels[0].value : 0;

    const item = {
      id: 0,
      listId: this.selectedList.id,
      priority: defaultPriority,
      title: '',
      done: false,
      backgroundColor: '#ffffff',
      tags: [],
    } as TodoItemDto;

    this.selectedList.items.push(item);
    const index = this.selectedList.items.length - 1;
    this.editItem(item, 'itemTitle' + index);
  }

  editItem(item: TodoItemDto, inputId: string): void {
    this.selectedItem = item;
    setTimeout(() => document.getElementById(inputId).focus(), 100);
  }

  updateItem(item: TodoItemDto, pressedEnter: boolean = false): void {
    const isNewItem = item.id === 0;

    if (!item.title.trim()) {
      this.deleteItem(item);
      return;
    }

    if (item.id === 0) {
      this.itemsClient
        .create({
          ...item,
          listId: this.selectedList.id,
        } as CreateTodoItemCommand)
        .subscribe(
          (result) => {
            item.id = result;
          },
          (error) => console.error(error)
        );
    } else {
      this.itemsClient.update(item.id, item).subscribe(
        () => console.log('Update succeeded.'),
        (error) => console.error(error)
      );
    }

    this.selectedItem = null;

    if (isNewItem && pressedEnter) {
      setTimeout(() => this.addItem(), 250);
    }
  }

  deleteItem(item: TodoItemDto, countDown?: boolean) {
    if (countDown) {
      if (this.deleting) {
        this.stopDeleteCountDown();
        return;
      }
      this.deleteCountDown = 3;
      this.deleting = true;
      this.deleteCountDownInterval = setInterval(() => {
        if (this.deleting && --this.deleteCountDown <= 0) {
          this.deleteItem(item, false);
        }
      }, 1000);
      return;
    }

    this.deleting = false;
    if (this.itemDetailsModalRef) {
      this.itemDetailsModalRef.hide();
    }

    if (item.id === 0) {
      const itemIndex = this.selectedList.items.indexOf(this.selectedItem);
      this.selectedList.items.splice(itemIndex, 1);
    } else {
      this.itemsClient.delete(item.id).subscribe({
        next: () => {
          this.selectedList.items = this.selectedList.items.filter(
            (t) => t.id !== item.id
          );
          console.log('Item moved to trash');
        },
        error: (error) => {
          console.error('Error deleting item', error);
        },
      });
    }
  }

  stopDeleteCountDown() {
    clearInterval(this.deleteCountDownInterval);
    this.deleteCountDown = 0;
    this.deleting = false;
  }

  addTag(): void {
    const currentTags = this.itemDetailsFormGroup.get('tags').value || [];
    if (this.newTag && !currentTags.includes(this.newTag)) {
      currentTags.push(this.newTag);
      this.itemDetailsFormGroup.get('tags').setValue(currentTags);
      this.newTag = '';
    }
  }

  removeTag(tag: string): void {
    const currentTags = this.itemDetailsFormGroup.get('tags').value || [];
    this.itemDetailsFormGroup
      .get('tags')
      .setValue(currentTags.filter((t) => t !== tag));
  }

  toggleFilterTag(tag: string): void {
    const tagIndex = this.filterTags.indexOf(tag);

    if (tagIndex !== -1) {
      this.filterTags.splice(tagIndex, 1);
    } else {
      this.filterTags.push(tag);
    }

    this.applyFilters();
  }

  updateSelectedList(): void {
    if (this.selectedList) {
      this.selectedList =
        this.lists.find((l) => l.id === this.selectedList.id) || this.lists[0];
    }
  }

  loadTodoLists(): void {
    this.listsClient.get().subscribe({
      next: (result) => {
        const lists = result.lists.map((list) =>
          Object.assign(new TodoListDto(), {
            ...list,
            items: list.items
              .filter((item) => !item.isDeleted)
              .map((item) =>
                Object.assign(new TodoItemDto(), {
                  ...item,
                  backgroundColor: item.backgroundColor || '#ffffff',
                  tags: item.tags || [],
                })
              ),
          })
        );

        this.unfilteredLists = lists;
        this.lists = [...lists];

        if (this.lists.length) {
          this.selectedList = this.lists[0];
        }
      },
      error: (error) => console.error('Error loading lists:', error),
    });
  }

  applyFilters(): void {
    const searchTermLower = this.searchTerm.toLowerCase();

    this.lists = this.unfilteredLists.map((originalList) => {
      let filteredItems = originalList.items;

      if (this.filterTags.length > 0) {
        filteredItems = filteredItems.filter(
          (item) =>
            item.tags && this.filterTags.every((tag) => item.tags.includes(tag))
        );
      }

      if (this.searchTerm.trim()) {
        filteredItems = filteredItems.filter(
          (item) =>
            item.title.toLowerCase().includes(searchTermLower) ||
            (item.note && item.note.toLowerCase().includes(searchTermLower))
        );
      }

      return Object.assign(new TodoListDto(), {
        ...originalList,
        items: filteredItems.map((item) =>
          Object.assign(new TodoItemDto(), item)
        ),
      });
    });

    this.updateSelectedList();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.applyFilters();
  }

  onSearchInput(): void {
    if (this.searchDebounceTimer) {
      clearTimeout(this.searchDebounceTimer);
    }

    this.searchDebounceTimer = setTimeout(() => {
      this.applyFilters();
      this.searchDebounceTimer = null;
    }, 300);
  }

  loadDeletedItems(): void {
    this.listsClient.getDeletedLists().subscribe({
      next: (lists) => (this.deletedLists = lists),
      error: (error) => console.error('Error loading deleted lists', error),
    });

    this.itemsClient.getDeletedItems().subscribe({
      next: (items) => (this.deletedItems = items),
      error: (error) => console.error('Error loading deleted items', error),
    });
  }

  restoreList(listId: number): void {
    const currentSelectedListId = this.selectedList?.id;

    this.listsClient.restoreList(listId).subscribe({
      next: () => {
        this.loadDeletedItems();

        this.listsClient.get().subscribe({
          next: (result) => {
            const lists = result.lists.map((list) =>
              Object.assign(new TodoListDto(), {
                ...list,
                items: list.items
                  .filter((item) => !item.isDeleted)
                  .map((item) =>
                    Object.assign(new TodoItemDto(), {
                      ...item,
                      backgroundColor: item.backgroundColor || '#ffffff',
                      tags: item.tags || [],
                    })
                  ),
              })
            );

            this.unfilteredLists = lists;
            this.lists = [...lists];

            this.selectedList =
              this.lists.find((l) => l.id === currentSelectedListId) ||
              this.lists[0];

            console.log('List restored successfully');
          },
          error: (error) => console.error('Error refreshing lists:', error),
        });
      },
      error: (error) => console.error('Error restoring list:', error),
    });
  }

  restoreItem(itemId: number): void {
    const currentSelectedListId = this.selectedList?.id;

    this.itemsClient.restoreItem(itemId).subscribe({
      next: () => {
        this.loadDeletedItems();

        this.listsClient.get().subscribe({
          next: (result) => {
            const lists = result.lists.map((list) =>
              Object.assign(new TodoListDto(), {
                ...list,
                items: list.items
                  .filter((item) => !item.isDeleted)
                  .map((item) =>
                    Object.assign(new TodoItemDto(), {
                      ...item,
                      backgroundColor: item.backgroundColor || '#ffffff',
                      tags: item.tags || [],
                    })
                  ),
              })
            );

            this.unfilteredLists = lists;
            this.lists = [...lists];

            this.selectedList =
              this.lists.find((l) => l.id === currentSelectedListId) ||
              this.lists[0];

            console.log('Item restored successfully');
          },
          error: (error) => console.error('Error refreshing lists: ', error),
        });
      },
      error: (error) => console.error('Error restoring item: ', error),
    });
  }

  toggleDeletedView(): void {
    this.showDeleted = !this.showDeleted;
    if (this.showDeleted) {
      this.loadDeletedItems();
    }
  }

  get deletedItemsForSelectedList(): any[] {
    return (
      this.deletedItems?.filter(
        (item) => item.listId === this.selectedList?.id
      ) || []
    );
  }
}

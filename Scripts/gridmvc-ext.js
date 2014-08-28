(function ($) {
    var plugin = GridMvc.prototype;

    // store copies of the original plugin functions before overwriting
    var functions = {};
    for (var i in plugin) {
        if (typeof (plugin[i]) === 'function') {
            functions[i] = plugin[i];
        }
    }


    // extend existing functionality of the gridmvc plugin
    $.extend(true, plugin, {
        applyFilterValues: function (initialUrl, columnName, values, skip) {
            var self = this;
            self.gridColumnFilters = null;
            var filters = self.jqContainer.find(".grid-filter");
            var url = URI(initialUrl).normalizeSearch().search();

            if (url.length > 0)
                url += "&";

            self.gridColumnFilters = "";
            if (!skip) {
                self.gridColumnFilters += this.getFilterQueryData(columnName, values);
            }

            if (this.options.multiplefilters) { //multiple filters enabled
                for (var i = 0; i < filters.length; i++) {
                    if ($(filters[i]).attr("data-name") != columnName) {
                        var filterData = this.parseFilterValues($(filters[i]).attr("data-filterdata"));
                        if (filterData.length == 0) continue;
                        if (url.length > 0) url += "&";
                        self.gridColumnFilters += this.getFilterQueryData($(filters[i]).attr("data-name"), filterData);
                    } else {
                        continue;
                    }
                }
            }

            url += self.gridColumnFilters;
            var fullSearch = url;
            if (fullSearch.indexOf("?") == -1) {
                fullSearch = "?" + fullSearch;
            }

            self.gridColumnFilters = fullSearch;

            self.currentPage = 1;

            if (self.gridFilterForm) {
                var formButton = $("#" + self.gridFilterForm.attr('id') + " input[type=submit],button[type=submit]")[0];
                var l = Ladda.create(formButton);
                l.start();
            }

            self.updateGrid(fullSearch, function () {
                if (l) {
                    l.stop();
                }
            });
        },
        ajaxify: function (options) {
            var self = this;
            self.currentPage = 1;
            self.loadPagedDataAction = options.getPagedData;
            self.loadDataAction = options.getData;
            self.gridFilterForm = options.gridFilterForm;
            self.gridSort = self.jqContainer.find("div.sorted a").attr('href');
            self.pageSetNum = 1;
            self.partitionSize = parseInt(self.jqContainer.find(".grid-pageSetLink").attr("data-partitionSize"));
            self.lastPageNum = parseInt(self.jqContainer.find(".grid-page-link:last").attr('data-page'));

            if (self.gridSort) {
                if (self.gridSort.indexOf("grid-dir=0") != -1) {
                    self.gridSort = self.gridSort.replace("grid-dir=0", "grid-dir=1");
                } else {
                    self.gridSort = self.gridSort.replace("grid-dir=1", "grid-dir=0");
                }
            }

            self.updateGrid = function (search, callback) {
                var gridQuery = "";

                if (self.gridFilterForm) {
                    $("#" + self.gridFilterForm.attr("id") + " input,select").each(function (index, item) {
                        if ($(item).attr('id')) {
                            var queryVal = $(item).attr('id') + "=" + $(item).val();
                            if (gridQuery !== "") {
                                gridQuery += "&" + queryVal;
                            } else {
                                gridQuery += queryVal;
                            }
                        }
                    });
                }

                if (search) {
                    gridQuery = search + "&" + gridQuery;
                } else {
                    gridQuery = "?" + gridQuery;
                }

                if (self.gridSort) {
                    if (self.gridSort.indexOf("?") != -1) {
                        self.gridSort = self.gridSort.substr(1);
                    }

                    gridQuery += "&" + self.gridSort;
                }

                gridQuery = URI(gridQuery).normalizeSearch().search();
                if (self.loadDataAction.indexOf("?") != -1) {
                    gridQuery = gridQuery.replace("?", "&");
                }

                var gridUrl = URI(self.loadDataAction + gridQuery).normalizeSearch().toString();

                $.ajax({
                    url: gridUrl,
                    type: 'get',
                    data: {},
                    contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                    async: true,
                    cache: false,
                    timeout: 20000
                }).done(function (response) {
                    self.jqContainer.html($('<div>' + response.Html + '</div>').find("div.grid-wrap"));
                    self.initFilters();
                    self.pageSetNum = 1;
                    self.notifyOnGridLoaded(response, $.Event("GridLoaded"));
                }).always(function (response) {
                    if (callback) {
                        callback();
                    }
                });
            };

            self.SetupGridHeaderEvents = function () {
                self.jqContainer.on('click', '.grid-header-title > a', function (e) {
                    self.gridSort = '';
                    e.preventDefault();
                    self.currentPage = 1;

                    if (self.gridFilterForm) {
                        var formButton = $("#" + self.gridFilterForm.attr('id') + " input[type=submit],button[type=submit]")[0];
                        var l = Ladda.create(formButton);
                        l.start();
                    }

                    // remove grid sort arrows
                    self.jqContainer.find(".grid-header-title").removeClass("sorted-asc");
                    self.jqContainer.find(".grid-header-title").removeClass("sorted-desc");

                    var mySearch = $(this).attr('href');
                    var isAscending = mySearch.indexOf("grid-dir=1") !== -1;
                    self.updateGrid(mySearch, function () {
                        if (l) {
                            l.stop();
                        }
                    });

                    // update link to sort in opposite direction
                    if (isAscending) {
                        $(this).attr('href', mySearch.replace("grid-dir=1", "grid-dir=0"));
                    } else {
                        $(this).attr('href', mySearch.replace("grid-dir=0", "grid-dir=1"));
                    }

                    // add new grid sort arrow
                    var newSortClass = isAscending ? "sorted-desc" : "sorted-asc";
                    $(this).parent(".grid-header-title").addClass(newSortClass);
                    $(this).parent(".grid-header-title").children("span").remove();
                    $(this).parent(".grid-header-title").append($("<span/>").addClass("grid-sort-arrow"));
                    self.gridSort = mySearch.substr(mySearch.match(/grid-column=\w+/).index);
                });
            };

            if (self.gridFilterForm) {
                self.gridFilterForm.on('submit', function (e) {
                    e.preventDefault();
                    self.currentPage = 1;
                    var formButton = $("#" + self.gridFilterForm.attr('id') + " input[type=submit],button[type=submit]")[0];
                    var l = Ladda.create(formButton);
                    l.start();
                    self.updateGrid(location.search, function () {
                        l.stop();
                    });
                });
            }

            self.loadNextPageSet = function () {
                // load next page set
                var pageSetNum = self.pageSetNum + 1;
                self.partitionSize = parseInt(self.jqContainer.find(".grid-pageSetLink").attr("data-partitionSize"));
                self.lastPageNum = parseInt(self.jqContainer.find(".grid-page-link:last").attr('data-page'));
                var nextPageNum = (pageSetNum - 1) * self.partitionSize + 2;

                if (nextPageNum <= self.lastPageNum) {
                    self.jqContainer.find(".grid-page-link").each(function (index, item) {
                        var currentPage = parseInt($(item).attr('data-page'));
                        if (currentPage > 1 && currentPage < self.lastPageNum) {
                            // loading next set of pages
                            if (nextPageNum < self.lastPageNum) {
                                $(item).show();
                                $(item).attr('data-page', nextPageNum).text(nextPageNum);
                            } else {
                                $(item).hide();
                            }

                            nextPageNum++;
                        }
                    });

                    if (pageSetNum == 2) {
                        self.jqContainer.find(".grid-pageSetLink.prev").show();
                    } else if (pageSetNum * self.partitionSize + 1 >= self.lastPageNum) {
                        self.jqContainer.find(".grid-pageSetLink.next").hide();
                    }

                    $(this).attr('data-pageset', pageSetNum + 1);
                    self.jqContainer.find(".grid-pageSetLink.prev").attr('data-pageset', pageSetNum - 1);
                    self.pageSetNum = pageSetNum;

                    if (self.pageSetNum * self.partitionSize >= self.lastPageNum) {
                        // hide next page set link
                        self.jqContainer.find(".grid-pageSetLink.next").hide();
                    }
                }
            };

            self.loadPreviousPageSet = function () {
                // load previous page set
                self.partitionSize = parseInt(self.jqContainer.find(".grid-pageSetLink").attr("data-partitionSize"));
                self.lastPageNum = parseInt(self.jqContainer.find(".grid-page-link:last").attr('data-page'));
                var pageSetNum = self.pageSetNum - 1;
                var incrementSize = self.partitionSize - 2;

                if ((pageSetNum * self.partitionSize) <= self.lastPageNum) {
                    var newPage = pageSetNum * self.partitionSize - incrementSize;

                    self.jqContainer.find(".grid-page-link").each(function (index, item) {
                        var currentPage = parseInt($(item).attr('data-page'));
                        if (currentPage > 1) {
                            if (currentPage < self.lastPageNum) {
                                // loading previous set of pages
                                $(item).show();
                                $(item).attr('data-page', newPage).text(newPage);
                                newPage++;
                            }
                        }
                    });

                    if (pageSetNum == 1) {
                        self.jqContainer.find(".grid-pageSetLink.prev").hide();
                    }

                    if (pageSetNum > 1) {
                        $(this).attr('data-pageset', pageSetNum - 1);
                    }

                    self.jqContainer.find(".grid-pageSetLink.next").attr('data-pageset', pageSetNum + 1);

                    if (pageSetNum * self.partitionSize < self.lastPageNum) {
                        self.jqContainer.find(".grid-pageSetLink.next").show();
                    }
                    self.pageSetNum = pageSetNum;
                }
            };

            self.setupPagerLinkEvents = function () {
                self.jqContainer.on("click", ".grid-next-page", function (e) {
                    e.preventDefault();
                    self.currentPage++;
                    self.loadPage();
                    if (self.currentPage >= self.partitionSize * self.pageSetNum + 2) {
                        // load next page set
                        self.loadNextPageSet();
                    }
                    self.jqContainer.find(".pagination li.active").removeClass("active").children("a").attr('href', '#');
                    self.jqContainer.find("a[data-page=" + self.currentPage + "]").parent("li").addClass("active");
                });

                self.jqContainer.on("click", ".grid-prev-page", function (e) {
                    e.preventDefault();
                    self.currentPage--;
                    self.loadPage();

                    if (self.currentPage > 1 &&
                        self.currentPage < self.partitionSize * (self.pageSetNum - 1) + 2) {
                        self.loadPreviousPageSet();
                    }

                    self.jqContainer.find(".pagination li.active").removeClass("active").children("a").attr('href', '#');
                    self.jqContainer.find("a[data-page=" + self.currentPage + "]").parent("li").addClass("active");
                });

                self.jqContainer.on("click", ".grid-page-link", function (e) {
                    e.preventDefault();
                    var pageNumber = $(this).attr('data-page');
                    var oldPageNumber = self.currentPage;
                    self.currentPage = pageNumber;
                    self.loadPage();
                    self.jqContainer.find(".pagination li.active").removeClass("active").children("a").attr('href', '#');
                    $(this).parent("li").addClass("active");

                    if (self.currentPage == 1 && oldPageNumber != 1) {
                        // load first page set
                        self.pageSetNum = 2;
                        self.loadPreviousPageSet();
                    } else if (self.currentPage == self.lastPageNum && self.currentPage != oldPageNumber) {
                        // load last page set
                        self.pageSetNum = Math.ceil(self.lastPageNum / self.partitionSize) - 1;
                        self.loadNextPageSet();
                        self.jqContainer.find(".grid-pageSetLink.prev").show();
                    }
                });

                self.jqContainer.on("click", ".grid-pageSetLink.next", function (e) {
                    e.preventDefault();
                    self.loadNextPageSet();

                    // reload new selected page
                    self.jqContainer.find("li.active .grid-page-link").click();
                });

                self.jqContainer.on("click", ".grid-pageSetLink.prev", function (e) {
                    e.preventDefault();
                    self.loadPreviousPageSet();

                    // reload new selected page
                    self.jqContainer.find("li.active .grid-page-link").click();
                });
            };

            self.loadPage = function () {
                var gridTableBody = self.jqContainer.find(".grid-footer").closest(".grid-wrap").find("tbody");
                var nextPageLink = self.jqContainer.find(".grid-next-page");
                var prevPageLink = self.jqContainer.find(".grid-prev-page");
                self.partitionSize = parseInt(self.jqContainer.find(".grid-pageSetLink").attr("data-partitionSize"));
                self.lastPageNum = parseInt(self.jqContainer.find(".grid-page-link:last").attr('data-page'));

                var gridQuery = "";

                if (self.gridFilterForm) {
                    $("#" + self.gridFilterForm.attr("id") + " input,select").each(function (index, item) {
                        if ($(item).attr('id')) {
                            gridQuery += "&" + $(item).attr('id') + "=" + $(item).val();
                        }
                    });
                }

                if (self.gridSort) {
                    gridQuery += "&" + self.gridSort.replace("?", "");
                }

                if (self.gridColumnFilters) {
                    gridQuery += "&" + self.gridColumnFilters.replace("?", "");
                }

                if (self.gridFilterForm) {
                    var formButton = $("#" + self.gridFilterForm.attr('id') + " input[type=submit],button[type=submit]")[0];
                    var l = Ladda.create(formButton);
                    l.start();
                }

                var pageQuery = self.pad(location.search) + self.currentPage;

                if (self.loadPagedDataAction.indexOf("?") !== -1) {
                    gridQuery = gridQuery.replace("?", "&");
                    pageQuery = pageQuery.replace("?", "&");
                }

                var gridUrl = URI(self.loadPagedDataAction + pageQuery + gridQuery).normalizeSearch().toString();

                $.get(gridUrl)
                    .done(function (response) {
                        gridTableBody.html("");
                        gridTableBody.append(response.Html);
                        if (!response.HasItems) {
                            nextPageLink.hide();
                        } else {
                            nextPageLink.show();
                        }

                        if (self.currentPage == 1) {
                            prevPageLink.hide();
                        } else {
                            prevPageLink.show();
                        }

                        if (l) {
                            l.stop();
                        }

                        self.notifyOnGridLoaded(response, $.Event("GridLoaded"));
                    })
                    .fail(function () {
                        alert("cannot load items");
                    });
            };

            this.pad = function (query) {
                if (query.length == 0) return "?page=";
                return query + "&page=";
            };

            self.SetupGridHeaderEvents();
            self.setupPagerLinkEvents();
        },
        onGridLoaded: function (func) {
            this.events.push({ name: "onGridLoaded", callback: func });
        },
        notifyOnGridLoaded: function (data, e) {
            e.data = data;
            this.notifyEvent("onGridLoaded", e);
        },
        refreshFullGrid: function () {
            var self = this;
            self.currentPage = 1;
            self.updateGrid(location.search, function () {
            });
        }
    });
})(jQuery);
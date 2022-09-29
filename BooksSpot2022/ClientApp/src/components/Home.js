import React, { Component } from 'react';

export class Home extends Component {
    static displayName = Home.name;

    constructor(props) {
        super(props);
        this.state = { books: [], loading: true };
    }

    componentDidMount() {
        this.populateBooksData();
    }

    static statusToString(bookStatus) {
        if (bookStatus == 0) {
            return 'Available';
        } else if (bookStatus == 1) {
            return 'Reserved';
        } else if (bookStatus == 2) {
            return 'Borrowed';
        }
        return bookStatus;
    }

    static renderBooksTable(books) {
        return (
            <table className='table table-striped'>
                <thead>
                    <tr>
                        <th>Title</th>
                        <th>Author</th>
                        <th>Publisher</th>
                        <th>Publishing Date</th>
                        <th>Genre</th>
                        <th>ISBN</th>
                        <th>Status</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {books.map(book =>
                        <tr key={book.id}>
                            <td>{book.title}</td>
                            <td>{book.author}</td>
                            <td>{book.publisher}</td>
                            <td>{book.publishingDate}</td>
                            <td>{book.genre}</td>
                            <td>{book.isbn}</td>
                            <td>{Home.statusToString(book.status)}</td>
                            <td>
                                <button type="button" class="btn btn-secondary m-1">Reserve</button>
                                <button type="button" class="btn btn-primary m-1">Borrow</button>
                            </td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : Home.renderBooksTable(this.state.books);

        return (
            <div>
                <h1>Books</h1>
                {contents}
            </div>
        );
    }

    async populateBooksData() {
        const response = await fetch('books');
        const data = await response.json();
        console.log(data);
        this.setState({ books: data, loading: false });
    }
}

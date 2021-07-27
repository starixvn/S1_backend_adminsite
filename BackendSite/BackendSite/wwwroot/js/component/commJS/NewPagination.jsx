import ReactPaginate from 'react-paginate';

export class NewPagination extends React.Component {
    constructor(props) {
        super(props);
    }

    ChangePage(pageNumber) {
        const selectedPage = pageNumber.selected;
        this.props.ChangePage(selectedPage+1);
    }

    render() {

        let data = this.props.Data;
        return (
            <nav className="paging" aria-label="Page navigation example">
                <span>
                    <tr>Total records: {data.rowCount}</tr>
                </span>
                <ReactPaginate
                    previousLabel={"<"}
                    nextLabel={">"}
                    breakLabel={"..."}
                    breakClassName={"page-item"}
                    breakLinkClassName={"page-link"}
                    pageCount={data.totalPages}
                    marginPagesDisplayed={2}
                    pageRangeDisplayed={10}
                    onPageChange={this.ChangePage.bind(this)}
                    containerClassName={"pagination"}
                    activeClassName={"page-item active"}
                    activeLinkClassName={"page-link"}
                    pageClassName={"page-item"}
                    pageLinkClassName={"page-link"}
                    nextClassName={"page-item"}
                    nextLinkClassName={"page-link"}
                    previousClassName={"page-item"}
                    previousLinkClassName={"page-link"}
                    forcePage={data.pageNumber-1}
                />
            </nav>
        )
    }
}        
import React from "react";
import ReactPaginate from 'react-paginate';

const Pagination = (props) => {
  let data = props.Data;
  let handlePageClick = (e) => {
    const selectedPage = e.selected;
    props.ChangePage(selectedPage+1);
  };
  return (
    <nav className="paging" aria-label="Page navigation example">
        <span>Total records: {data.rowCount}</span>
      <ReactPaginate
        previousLabel={"<"}
        nextLabel={">"}
        breakLabel={"..."}
        breakClassName={"page-item"}
        breakLinkClassName={"page-link"}
        pageCount={data.totalPages}
        marginPagesDisplayed={2}
        pageRangeDisplayed={10}
        onPageChange={handlePageClick}
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
  );
};

export default Pagination;

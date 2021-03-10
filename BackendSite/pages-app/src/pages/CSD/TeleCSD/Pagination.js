import React from "react";

const Pagination = (props) => {
  let data = props.Data;
  let PageNum = [];

  for (let i = 1; i <= data.totalPages; i++) {
    PageNum.push(
      <li
        className={"page-item" + (data.pageNumber == i ? " active" : "")}
        key={i}
      >
        <a
          className="page-link"
          onClick={
            data.pageNumber == i
              ? null
              : () => {
                  props.ChangePage(i);
                }
          }
        >
          {i}
        </a>
      </li>
    );
  }

  let disableNext = data.pageNumber == data.totalPages || data.totalPages == 0;

  return (
    <nav className="paging" aria-label="Page navigation example">
      <span>Total records: {data.rowCount}</span>
      <ul className="pagination">
        <li className={"page-item" + (data.pageNumber == 1 ? " disabled" : "")}>
          <a
            className="page-link"
            tabIndex="-1"
            aria-disabled={data.pageNumber == 1}
            onClick={
              data.pageNumber == 1
                ? null
                : () => {
                    props.ChangePage(data.pageNumber - 1);
                  }
            }
          >
            <span aria-hidden="true">&laquo;</span>
            <span className="sr-only">Previous</span>
          </a>
        </li>
        {PageNum}
        <li className={"page-item" + (disableNext ? " disabled" : "")}>
          <a
            className="page-link"
            tabIndex="+1"
            aria-disabled={disableNext}
            onClick={
              disableNext
                ? null
                : () => {
                    props.ChangePage(data.pageNumber + 1);
                  }
            }
          >
            <span aria-hidden="true">&raquo;</span>
            <span className="sr-only">Next</span>
          </a>
        </li>
      </ul>
    </nav>
  );
};

export default Pagination;
